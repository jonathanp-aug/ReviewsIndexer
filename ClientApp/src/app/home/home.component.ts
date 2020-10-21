import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import ProductIndexationState from '../../core/models/productIndexationState';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';

const UPDATE_DELAY = 300;

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {

	public productStates: Array<ProductIndexationState>;
	public selectedState?: ProductIndexationState;
	public baseUrl: string;
	public http: HttpClient;
	public requestIndexationForm: FormGroup;
	public asinsControl: FormControl;

	constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
		this.baseUrl = baseUrl;
		this.http = http;
		this.productStates = [];

		var formBuilder = new FormBuilder();

		this.asinsControl = new FormControl('');
		this.requestIndexationForm = formBuilder.group({ asins: this.asinsControl });
	}

	queryAndUpdateWaitingStates() {
		let waitingStates = this.productStates
			.filter(state => state.indexationStatus === 0 || state.indexationStatus === 1)
			.map(state => state.productAsin)
			.join(",");

		this.http.get<ProductIndexationState[]>(this.baseUrl + 'product/list?asins=' + waitingStates).subscribe(result => {
			this.updateStates(result);
		}, error => console.error(error));
	}

	getBadgeLevel(state: ProductIndexationState) {
		switch (state.indexationStatus) {
			case 0: return "warning";
			case 1: return "warning";
			case 2: return "success";
			case 3: return "danger";
		}
	}

	selectState(state: ProductIndexationState) {
		this.selectedState = state;
	}

	onRequestIndexation() {
		var formData = new FormData();
		formData.append('asins', this.asinsControl.value);

		this.http.post<ProductIndexationState[]>(this.baseUrl + 'product/RequestIndexation', formData).subscribe(result => {
			this.requestIndexationForm.reset();
			
			if (!result || result.length === 0)
				return;

			this.updateStates(result);
		}, error => console.error(error));
	}

	updateStates(newStates) {
		var tmpStates = [...this.productStates];
		newStates.forEach(state => {
			for (let i=0; i<tmpStates.length; i++)
				if (state.productAsin === tmpStates[i].productAsin) {
					tmpStates[i] = state;
					if (this.selectedState && state.productAsin === this.selectedState.productAsin)
						this.selectedState = state;
					return;
				}
			
			tmpStates.push(state);
		});

		this.productStates = tmpStates;

		if (this.productStates.filter(state => state.indexationStatus === 0 || state.indexationStatus === 1).length > 0)
				setTimeout(() => this.queryAndUpdateWaitingStates(), UPDATE_DELAY);
	}
}
