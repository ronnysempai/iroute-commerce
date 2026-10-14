import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CommerceService } from '../../core/services/commerce.service';
import { ProcessResult } from '../../core/models/commerce.models';

/** Pantalla para procesar los registros de una fecha (pc_processdate). */
@Component({
  selector: 'app-process',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './process.component.html',
  styleUrl: './process.component.css'
})
export class ProcessComponent {
  processDate = '';
  result: ProcessResult | null = null;
  error = '';
  loading = false;

  constructor(private readonly commerce: CommerceService) {}

  process(): void {
    if (!this.processDate) {
      this.error = 'Selecciona una fecha de proceso.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.result = null;

    this.commerce.process(this.processDate).subscribe({
      next: (result) => {
        this.result = result;
        this.loading = false;
      },
      error: (error) => {
        this.error = error?.error?.message ?? 'No se pudo procesar la fecha.';
        this.loading = false;
      }
    });
  }
}
