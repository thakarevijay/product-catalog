import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { ProductService } from '../../../../core/services/product.service';
import { CategoryService } from '../../../../core/services/category.service';
import { Category } from '../../../../core/models/product.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatCardModule
  ],
  template: `
    <div class="container">
      <div class="header">
        <button mat-icon-button routerLink="/products">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>{{ isEditMode() ? 'Edit Product' : 'New Product' }}</h1>
      </div>

      @if (loading()) {
        <div class="loading">
          <mat-spinner diameter="40"></mat-spinner>
        </div>
      }

      @if (!loading()) {
        <mat-card>
          <mat-card-content>
            <form [formGroup]="form" (ngSubmit)="onSubmit()">

              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Product Name</mat-label>
                  <input matInput formControlName="name" placeholder="Enter product name">
                  @if (form.get('name')?.hasError('required') && form.get('name')?.touched) {
                    <mat-error>Name is required</mat-error>
                  }
                  @if (form.get('name')?.hasError('maxlength')) {
                    <mat-error>Name must not exceed 200 characters</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>SKU</mat-label>
                  <input matInput formControlName="sku" placeholder="e.g. PROD-001">
                  @if (form.get('sku')?.hasError('required') && form.get('sku')?.touched) {
                    <mat-error>SKU is required</mat-error>
                  }
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Description</mat-label>
                <textarea matInput formControlName="description" rows="3"
                          placeholder="Optional product description"></textarea>
              </mat-form-field>

              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Price (€)</mat-label>
                  <input matInput type="number" formControlName="price" placeholder="0.00">
                  @if (form.get('price')?.hasError('required') && form.get('price')?.touched) {
                    <mat-error>Price is required</mat-error>
                  }
                  @if (form.get('price')?.hasError('min')) {
                    <mat-error>Price must be greater than 0</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Stock Quantity</mat-label>
                  <input matInput type="number" formControlName="stockQuantity" placeholder="0">
                  @if (form.get('stockQuantity')?.hasError('min')) {
                    <mat-error>Stock cannot be negative</mat-error>
                  }
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Category</mat-label>
                  <mat-select formControlName="categoryId">
                    @for (category of categories(); track category.id) {
                      <mat-option [value]="category.id">{{ category.name }}</mat-option>
                    }
                  </mat-select>
                  @if (form.get('categoryId')?.hasError('required') && form.get('categoryId')?.touched) {
                    <mat-error>Category is required</mat-error>
                  }
                </mat-form-field>

                @if (isEditMode()) {
                  <mat-form-field appearance="outline">
                    <mat-label>Status</mat-label>
                    <mat-select formControlName="status">
                      <mat-option value="Active">Active</mat-option>
                      <mat-option value="Inactive">Inactive</mat-option>
                      <mat-option value="Discontinued">Discontinued</mat-option>
                    </mat-select>
                  </mat-form-field>
                }
              </div>

              <div class="form-actions">
                <button mat-button type="button" routerLink="/products">Cancel</button>
                <button mat-raised-button color="primary" type="submit"
                        [disabled]="form.invalid || saving()">
                  @if (saving()) {
                    <mat-spinner diameter="20"></mat-spinner>
                  } @else {
                    {{ isEditMode() ? 'Save Changes' : 'Create Product' }}
                  }
                </button>
              </div>

            </form>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .container { padding: 24px; max-width: 800px; margin: 0 auto; }
    .header { display: flex; align-items: center; gap: 8px; margin-bottom: 24px; }
    .header h1 { margin: 0; font-size: 24px; font-weight: 500; }
    .loading { display: flex; justify-content: center; padding: 48px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .full-width { width: 100%; }
    mat-form-field { width: 100%; }
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 16px; }
    mat-card { padding: 8px; }
  `]
})
export class ProductFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  isEditMode = signal(false);
  loading = signal(false);
  saving = signal(false);
  categories = signal<Category[]>([]);
  productId: number | null = null;

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    description: [''],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stockQuantity: [0, [Validators.min(0)]],
    categoryId: [null as number | null, Validators.required],
    status: ['Active']
  });

  ngOnInit(): void {
    this.loadCategories();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.productId = +id;
      this.loadProduct(+id);
    }
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats),
      error: (err) => this.snackBar.open('Failed to load categories', 'Close', { duration: 3000 })
    });
  }

  loadProduct(id: number): void {
    this.loading.set(true);
    this.productService.getProductById(id).subscribe({
      next: (product) => {
        this.form.patchValue({
          name: product.name,
          sku: product.sku,
          description: product.description,
          price: product.price,
          stockQuantity: product.stockQuantity,
          status: product.status
        });
        this.loading.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.message, 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    const value = this.form.value;

    if (this.isEditMode() && this.productId) {
      this.productService.updateProduct(this.productId, {
        name: value.name!,
        sku: value.sku!,
        description: value.description || undefined,
        price: value.price!,
        stockQuantity: value.stockQuantity!,
        status: value.status!,
        categoryId: value.categoryId!
      }).subscribe({
        next: () => {
          this.snackBar.open('Product updated successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/products']);
        },
        error: (err) => {
          this.snackBar.open(err.message, 'Close', { duration: 3000 });
          this.saving.set(false);
        }
      });
    } else {
      this.productService.createProduct({
        name: value.name!,
        sku: value.sku!,
        description: value.description || undefined,
        price: value.price!,
        stockQuantity: value.stockQuantity!,
        categoryId: value.categoryId!
      }).subscribe({
        next: () => {
          this.snackBar.open('Product created successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/products']);
        },
        error: (err) => {
          this.snackBar.open(err.message, 'Close', { duration: 3000 });
          this.saving.set(false);
        }
      });
    }
  }
}
