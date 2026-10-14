import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

import { CommerceService } from '../../core/services/commerce.service';
import { CsvPreview, UploadResult } from '../../core/models/commerce.models';

const MAX_PREVIEW_ROWS = 50;

/** Pantalla de carga del archivo CSV con vista previa. */
@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.css'
})
export class UploadComponent {
  selectedFile: File | null = null;
  preview: CsvPreview | null = null;
  result: UploadResult | null = null;
  error = '';
  loading = false;

  private readonly fileNamePattern = /^commerce_\d{8}\.csv$/i;

  constructor(private readonly commerce: CommerceService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.selectedFile = file;
    this.preview = null;
    this.result = null;
    this.error = '';

    if (!file) {
      return;
    }

    if (!this.fileNamePattern.test(file.name)) {
      this.selectedFile = null;
      this.error =
        'El archivo debe llamarse commerce_DDMMYYYY.csv (por ejemplo commerce_13102026.csv).';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.preview = this.parseCsv(String(reader.result ?? ''));
    };
    reader.onerror = () => {
      this.error = 'No se pudo leer el archivo seleccionado.';
    };
    reader.readAsText(file);
  }

  upload(): void {
    if (!this.selectedFile) {
      this.error = 'Selecciona un archivo primero.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.result = null;

    this.commerce.upload(this.selectedFile).subscribe({
      next: (result) => {
        this.result = result;
        this.loading = false;
      },
      error: (error) => {
        this.error = error?.error?.message ?? 'No se pudo cargar el archivo.';
        this.loading = false;
      }
    });
  }

  private parseCsv(text: string): CsvPreview {
    const lines = text.split(/\r?\n/).filter((line) => line.trim().length > 0);

    if (lines.length === 0) {
      return { headers: [], rows: [] };
    }

    const headers = this.splitLine(lines[0]);
    const rows = lines.slice(1, MAX_PREVIEW_ROWS + 1).map((line) => this.splitLine(line));

    return { headers, rows };
  }

  private splitLine(line: string): string[] {
    const values: string[] = [];
    let current = '';
    let inQuotes = false;

    for (const char of line) {
      if (char === '"') {
        inQuotes = !inQuotes;
      } else if (char === ',' && !inQuotes) {
        values.push(current);
        current = '';
      } else {
        current += char;
      }
    }
    values.push(current);

    return values.map((value) => value.trim());
  }
}
