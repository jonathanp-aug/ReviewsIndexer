import { Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import ProductIndexationState from '../../core/models/productIndexationState';

@Component({
	selector: 'product-reviews',
	templateUrl: './product-reviews.component.html'
})
export class ProductReviewsComponent {
	@Input() state: ProductIndexationState;

	public orderField: string;
	public orderAsc: boolean;

	constructor(public datePipe: DatePipe) { }

	orderReviews(field: string, asc: boolean) {
		if (this.state.indexedReviews) {
			this.orderField = field;
			this.orderAsc = asc;
			this.state.indexedReviews.sort((a, b) => {
				if (a[field] > b[field])
					return asc ? 1 : -1;
				if (a[field] < b[field])
					return asc ? -1 : 1;
				return 0;
			});
		}
	}

	getBtnOrderClass(field: string, asc: boolean) {
		return this.orderField === field && this.orderAsc === asc ? 'warning' : 'secondary'
	}

	formatDate(date: string) {
		return this.datePipe.transform(new Date(date), 'yyyy-MM-dd');
	}
}

