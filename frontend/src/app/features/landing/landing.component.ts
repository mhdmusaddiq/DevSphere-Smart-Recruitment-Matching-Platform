import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { BrandWordmarkComponent } from '../../shared/brand/brand-wordmark.component';
import { AppIconComponent } from '../../shared/icons/app-icon.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [AppIconComponent, BrandWordmarkComponent, RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingComponent {}
