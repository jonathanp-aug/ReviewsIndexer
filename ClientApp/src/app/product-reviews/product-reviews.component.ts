import { Component, Input } from '@angular/core';
import ProductIndexationState from '../../core/models/productIndexationState';

@Component({
	selector: 'product-reviews',
	templateUrl: './product-reviews.component.html'
})
export class ProductReviewsComponent {
	@Input() productStates: Array<ProductIndexationState>;
	@Input() selected: string;

	public state: ProductIndexationState;

	ngOnInit() {
		this.state = this.productStates.filter(s => s.productAsin === this.selected)[0];
	}

	ngOnChanges() {
		this.state = this.productStates.filter(s => s.productAsin === this.selected)[0];
	}
}
