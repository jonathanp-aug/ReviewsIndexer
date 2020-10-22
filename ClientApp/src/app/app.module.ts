import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common'
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { ProductReviewsComponent } from './product-reviews/product-reviews.component';
import { ReviewTableComponent } from './review-table/review-table.component';

@NgModule({
	declarations: [
		AppComponent,
		NavMenuComponent,
		HomeComponent,
		ReviewTableComponent,
		ProductReviewsComponent
	],
	imports: [
		BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		RouterModule.forRoot([
			{ path: '', component: HomeComponent, pathMatch: 'full' },
		])
	],
	providers: [DatePipe],
	bootstrap: [AppComponent]
})
export class AppModule { }
