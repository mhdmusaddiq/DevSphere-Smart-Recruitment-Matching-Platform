import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { TransportErrorPresenter } from './core/errors/transport-error-presenter.service';
import { AppIconComponent } from './shared/icons/app-icon.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AppIconComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  readonly transportErrors = inject(TransportErrorPresenter);
}
