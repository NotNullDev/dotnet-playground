import createClient from 'openapi-fetch';
import type { paths } from '../schema';

export const { GET, POST, DELETE } = createClient<paths>({
	baseUrl: 'http://localhost:5193/'
});
