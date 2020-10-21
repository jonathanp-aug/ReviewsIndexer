import Review from './review';

export default interface ProductIndexationState {
	productAsin: string
	indexationStatus: number,
	errorMessage?: string,
	
	name?: string,
	url?: string,
	by?: string,
	ratingAverage?: string,
	ratingTotalCount?: string,
	reviewTotalCount?: string,
	indexedReviews?: Review[],
}