import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import {
  FormControl,
  ReactiveFormsModule
} from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import {
  finalize,
  forkJoin,
  Observable
} from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminOccupationConcept,
  AdminSkillAlias,
  AdminSkillConcept
} from '../data/admin.models';

type CatalogueTab =
  | 'skills'
  | 'aliases'
  | 'occupations';

type CatalogueRecord =
  | AdminSkillConcept
  | AdminSkillAlias
  | AdminOccupationConcept;

interface PendingStatusChange {
  kind: CatalogueTab;
  id: string;
  label: string;
  currentActive: boolean;
}

@Component({
  selector: 'app-admin-catalogue',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-catalogue.component.html',
  styleUrl: './admin-catalogue.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminCatalogueComponent
  implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly searchControl =
    new FormControl('', { nonNullable: true });

  readonly statusControl =
    new FormControl('', { nonNullable: true });

  readonly activeTab =
    signal<CatalogueTab>('skills');

  readonly skills =
    signal<AdminSkillConcept[]>([]);

  readonly aliases =
    signal<AdminSkillAlias[]>([]);

  readonly occupations =
    signal<AdminOccupationConcept[]>([]);

  readonly loading = signal(true);
  readonly errorMessage =
    signal<string | null>(null);

  readonly actionMessage =
    signal<string | null>(null);
  readonly actionError =
    signal<string | null>(null);

  readonly pendingChange =
    signal<PendingStatusChange | null>(null);

  readonly updating = signal(false);

  readonly filteredSkills = computed(() =>
    this.filterRecords(
      this.skills(),
      record => record.name
    )
  );

  readonly filteredAliases = computed(() =>
    this.filterRecords(
      this.aliases(),
      record =>
        `${record.alias} ${record.skillConceptName}`
    )
  );

  readonly filteredOccupations = computed(() =>
    this.filterRecords(
      this.occupations(),
      record => record.name
    )
  );

  readonly shownCount = computed(() => {
    if (this.activeTab() === 'skills') {
      return this.filteredSkills().length;
    }

    if (this.activeTab() === 'aliases') {
      return this.filteredAliases().length;
    }

    return this.filteredOccupations().length;
  });

  readonly activeCount = computed(() => {
    if (this.activeTab() === 'skills') {
      return this.filteredSkills()
        .filter(item => item.isActive)
        .length;
    }

    if (this.activeTab() === 'aliases') {
      return this.filteredAliases()
        .filter(item =>
          item.isActive &&
          item.skillConceptIsActive
        )
        .length;
    }

    return this.filteredOccupations()
      .filter(item => item.isActive)
      .length;
  });

  ngOnInit(): void {
    this.load();
  }

  setTab(tab: CatalogueTab): void {
    this.activeTab.set(tab);
    this.searchControl.setValue('', {
      emitEvent: false
    });
    this.statusControl.setValue('', {
      emitEvent: false
    });
    this.actionMessage.set(null);
    this.actionError.set(null);
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      skills: this.api.getSkillConcepts(true),
      aliases: this.api.getSkillAliases(true),
      occupations:
        this.api.getOccupationConcepts(true)
    })
      .pipe(
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: result => {
          this.skills.set(result.skills);
          this.aliases.set(result.aliases);
          this.occupations.set(
            result.occupations
          );
        },
        error: () => {
          this.errorMessage.set(
            'AptLens could not load the administration catalogue. Try again.'
          );
        }
      });
  }

  requestSkillChange(
    item: AdminSkillConcept
  ): void {
    this.pendingChange.set({
      kind: 'skills',
      id: item.id,
      label: item.name,
      currentActive: item.isActive
    });
  }

  requestAliasChange(
    item: AdminSkillAlias
  ): void {
    this.pendingChange.set({
      kind: 'aliases',
      id: item.id,
      label: item.alias,
      currentActive: item.isActive
    });
  }

  requestOccupationChange(
    item: AdminOccupationConcept
  ): void {
    this.pendingChange.set({
      kind: 'occupations',
      id: item.id,
      label: item.name,
      currentActive: item.isActive
    });
  }

  cancelChange(): void {
    this.pendingChange.set(null);
  }

  confirmChange(): void {
    const pending = this.pendingChange();

    if (!pending || this.updating()) {
      return;
    }

    const nextActive =
      !pending.currentActive;

    this.updating.set(true);
    this.actionMessage.set(null);
    this.actionError.set(null);

    this.statusRequest(
      pending,
      nextActive
    )
      .pipe(
        finalize(() => {
          this.updating.set(false);
        })
      )
      .subscribe({
        next: updated => {
          this.replaceRecord(
            pending.kind,
            updated
          );
          this.pendingChange.set(null);
          this.actionMessage.set(
            `${pending.label} is now ${
              nextActive ? 'active' : 'inactive'
            }.`
          );
        },
        error: (error: HttpErrorResponse) => {
          this.pendingChange.set(null);

          if (error.status === 404) {
            this.actionError.set(
              'The selected catalogue record no longer exists. Refresh the catalogue.'
            );
            return;
          }

          this.actionError.set(
            'AptLens could not update this catalogue status. Refresh and try again.'
          );
        }
      });
  }

  aliasAvailable(
    alias: AdminSkillAlias
  ): boolean {
    return (
      alias.isActive &&
      alias.skillConceptIsActive
    );
  }

  private statusRequest(
    pending: PendingStatusChange,
    nextActive: boolean
  ): Observable<CatalogueRecord> {
    if (pending.kind === 'skills') {
      return this.api.setSkillConceptStatus(
        pending.id,
        nextActive
      );
    }

    if (pending.kind === 'aliases') {
      return this.api.setSkillAliasStatus(
        pending.id,
        nextActive
      );
    }

    return this.api.setOccupationConceptStatus(
      pending.id,
      nextActive
    );
  }

  private replaceRecord(
    kind: CatalogueTab,
    updated: CatalogueRecord
  ): void {
    if (kind === 'skills') {
      const value =
        updated as AdminSkillConcept;

      this.skills.update(items =>
        items.map(item =>
          item.id === value.id ? value : item
        )
      );
      return;
    }

    if (kind === 'aliases') {
      const value =
        updated as AdminSkillAlias;

      this.aliases.update(items =>
        items.map(item =>
          item.id === value.id ? value : item
        )
      );
      return;
    }

    const value =
      updated as AdminOccupationConcept;

    this.occupations.update(items =>
      items.map(item =>
        item.id === value.id ? value : item
      )
    );
  }

  private filterRecords<T extends { isActive: boolean }>(
    records: T[],
    label: (record: T) => string
  ): T[] {
    const q =
      this.searchControl.value
        .trim()
        .toLowerCase();

    const status =
      this.statusControl.value;

    return records.filter(record => {
      const matchesSearch =
        !q ||
        label(record)
          .toLowerCase()
          .includes(q);

      const matchesStatus =
        !status ||
        (status === 'active'
          ? record.isActive
          : !record.isActive);

      return matchesSearch && matchesStatus;
    });
  }
}
