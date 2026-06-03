export interface Product {
  id: number;
  name: string;
  sku: string;
  description?: string;
  price: number;
  stockQuantity: number;
  status: string;
  categoryName: string;
  createdAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  description?: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
}

export interface UpdateProductRequest {
  name: string;
  sku: string;
  description?: string;
  price: number;
  stockQuantity: number;
  status: string;
  categoryId: number;
}

export interface Category {
  id: number;
  name: string;
}
