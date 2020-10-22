import { Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import Review from '../../core/models/review';
import ProductIndexationState from '../../core/models/productIndexationState';

@Component({
	selector: 'review-table',
	templateUrl: './review-table.component.html'
})
export class ReviewTableComponent {
	@Input() productStates: Array<ProductIndexationState>;
	@Input() showProductAsin: boolean = false;

	public displayedReviews: Array<Review>;
	public orderField: string;
	public orderAsc: boolean = true;
	public searchText: string;

	constructor(public datePipe: DatePipe) { }

	ngOnInit () {
		this.displayedReviews = this.buildReviewsList();
	}

	ngOnChanges() {
		this.displayedReviews = this.buildReviewsList();
	}

	buildReviewsList() {
		if (!this.productStates || this.productStates.length === 0)
			return [];

		var tmp = this.productStates
			.filter(state => state.indexationStatus === 2)
			.reduce((a,b) => { return a.concat(b.indexedReviews)}, []);

		if (this.searchText && this.searchText !== '')
			tmp = tmp.filter(review => {
				return review.title.toLowerCase().indexOf(this.searchText) >= 0
				 || review.content.toLowerCase().indexOf(this.searchText) >= 0;
			});
		
		if (this.orderField)
			tmp = tmp.sort((a, b) => {
				if (a[this.orderField] > b[this.orderField])
					return this.orderAsc ? 1 : -1;
				if (a[this.orderField] < b[this.orderField])
					return this.orderAsc ? -1 : 1;
				return 0;
			});

		return tmp;
	}

	orderReviews(field: string, asc: boolean) {
		this.orderField = field;
		this.orderAsc = asc;
		this.displayedReviews = this.buildReviewsList();
	}

	getBtnOrderClass(field: string, asc: boolean) {
		return this.orderField === field && this.orderAsc === asc ? 'warning' : 'secondary'
	}

	formatDate(date: string) {
		return this.datePipe.transform(new Date(date), 'yyyy-MM-dd');
	}

	onSearchTextChange($event) {
		this.searchText = $event.target.value.toLowerCase();
		this.displayedReviews = this.buildReviewsList();
	}
}

