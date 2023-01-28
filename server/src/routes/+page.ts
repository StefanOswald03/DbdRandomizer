import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
	const AllPerksUrl = 'https://www.themealdb.com/api/json/v1/1/categories.php';
	const response = await fetch(AllPerksUrl);
	console.log('Response:');
	console.log(response);
	const data = response.json();
	console.log('Data:');
	console.log(data);
};
