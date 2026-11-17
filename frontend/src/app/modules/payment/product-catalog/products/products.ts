import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-products',
  styleUrl: './products.scss',
  templateUrl: './products.html',
})
export class Products {
  protected readonly products = httpResource<ProductDto[]>(() => '/api/payment/products');
}
