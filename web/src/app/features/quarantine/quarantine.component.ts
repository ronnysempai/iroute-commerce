import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';

import { CommerceService } from '../../core/services/commerce.service';
import { QuarantineRecord } from '../../core/models/commerce.models';

/** Pantalla con la lista de registros en cuarentena y su motivo. */
@Component({
  selector: 'app-quarantine',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quarantine.component.html',
  styleUrl: './quarantine.component.css'
})
export class QuarantineComponent implements OnInit {
  records: QuarantineRecord[] = [];
  loading = false;
  error = '';

  constructor(private readonly commerce: CommerceService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';

    this.commerce.getQuarantine().subscribe({
      next: (records) => {
        this.records = records;
        this.loading = false;
      },
      error: (error) => {
        this.error = error?.error?.message ?? 'No se pudo obtener la lista de cuarentena.';
        this.loading = false;
      }
    });
  }
}
