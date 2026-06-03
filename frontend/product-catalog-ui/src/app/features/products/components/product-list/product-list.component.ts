import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductService } from '../../../../core/services/product.service';
import { Product } from '../../../../core/models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule
  ],
  template: `
    <div class="container">
      <div class="header">
        <h1>Product Catalog</h1>
        <button mat-raised-button color="primary" routerLink="/products/new">
          <mat-icon>add</mat-icon>
          Add Product
        </button>
      </div>

      <!-- Search -->
      <mat-form-field appearance="outline" class="search-field">
        <mat-label>Search products</mat-label>
        <input matInput [(ngModel)]="searchTerm" (ngModelChange)="onSearchChange($event)"
               placeholder="Search by name, SKU or category...">
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>

      <!-- Loading -->
      @if (loading()) {
        <div class="loading">
          <mat-spinner diameter="40"></mat-spinner>
        </div>
      }

      <!-- Table -->
      @if (!loading()) {
        <table mat-table [dataSource]="products()" class="mat-elevation-z2">

          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let product">{{ product.name }}</td>
          </ng-container>

          <ng-container matColumnDef="sku">
            <th mat-header-cell *matHeaderCellDef>SKU</th>
            <td mat-cell *matCellDef="let product">
              <code>{{ product.sku }}</code>
            </td>
          </ng-container>

          <ng-container matColumnDef="category">
            <th mat-header-cell *matHeaderCellDef>Category</th>
            <td mat-cell *matCellDef="let product">{{ product.categoryName }}</td>
          </ng-container>

          <ng-container matColumnDef="price">
            <th mat-header-cell *matHeaderCellDef>Price</th>
            <td mat-cell *matCellDef="let product">{{ product.price | currency:'EUR' }}</td>
          </ng-container>

          <ng-container matColumnDef="stock">
            <th mat-header-cell *matHeaderCellDef>Stock</th>
            <td mat-cell *matCellDef="let product">
              <span [class]="product.stockQuantity > 0 ? 'stock-ok' : 'stock-empty'">
                {{ product.stockQuantity }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let product">
              <span [class]="'status-badge status-' + product.status.toLowerCase()">
                {{ product.status }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let product">
              <button mat-icon-button color="primary"
                      [routerLink]="['/products', product.id, 'edit']"
                      matTooltip="Edit product">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn"
                      (click)="confirmDelete(product)"
                      matTooltip="Delete product">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

          <tr class="mat-row" *matNoDataRow>
            <td class="mat-cell no-data" [attr.colspan]="displayedColumns.length">
              No products found
            </td>
          </tr>
        </table>

        <mat-paginator
          [length]="totalCount()"
          [pageSize]="pageSize"
          [pageSizeOptions]="[5, 10, 25]"
          (page)="onPageChange($event)"
          showFirstLastButtons>
        </mat-paginator>
      }
    </div>
  `,
  styles: [`
    .container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .header h1 { margin: 0; font-size: 28px; font-weight: 500; }
    .search-field { width: 100%; margin-bottom: 16px; }
    .loading { display: flex; justify-content: center; padding: 48px; }
    table { width: 100%; }
    code { background: #f5f5f5; padding: 2px 6px; border-radius: 4px; font-size: 12px; }
    .stock-ok { color: #2e7d32; font-weight: 500; }
    .stock-empty { color: #c62828; font-weight: 500; }
    .status-badge { padding: 4px 10px; border-radius: 12px; font-size: 12px; font-weight: 500; }
    .status-active { background: #e8f5e9; color: #2e7d32; }
    .status-inactive { background: #fff3e0; color: #e65100; }
    .status-discontinued { background: #fce4ec; color: #c62828; }
    .no-data { text-align: center; padding: 48px; color: #9e9e9e; }
    th.mat-header-cell { font-weight: 600; color: #424242; }
  `]
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly searchSubject = new Subject<string>();

  displayedColumns = ['name', 'sku', 'category', 'price', 'stock', 'status', 'actions'];

  products = signal<Product[]>([]);
  totalCount = signal(0);
  loading = signal(false);

  searchTerm = '';
  currentPage = 1;
  pageSize = 10;

  constructor() {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntilDestroyed()
    ).subscribe(search => {
      this.currentPage = 1;
      this.loadProducts(search);
    });
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(search?: string): void {
    this.loading.set(true);
    this.productService.getProducts(this.currentPage, this.pageSize, search || this.searchTerm || undefined)
      .subscribe({
        next: (result) => {
          this.products.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          this.snackBar.open(err.message, 'Close', { duration: 3000 });
          this.loading.set(false);
        }
      });
  }

  onSearchChange(value: string): void {
    this.searchSubject.next(value);
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadProducts();
  }

  confirmDelete(product: Product): void {
    if (confirm(`Delete "${product.name}"?`)) {
      this.productService.deleteProduct(product.id).subscribe({
        next: () => {
          this.snackBar.open('Product deleted', 'Close', { duration: 3000 });
          this.loadProducts();
        },
        error: (err) => {
          this.snackBar.open(err.message, 'Close', { duration: 3000 });
        }
      });
    }
  }
}
