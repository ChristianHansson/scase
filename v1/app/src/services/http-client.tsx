

class HttpClient{
	baseUri: string | null = null;
	constructor() {
		this.baseUri = 'https://localhost:7219';
	}

	async get(url: string) {
		url = url.startsWith('/') ? url : `/${url}`
		const response = await fetch(`${this.baseUri}${url}`, {
			method: 'GET',
			headers: {
				'Content-Type': 'application/json',
				'X-Api-Key': 'dev-local-api-key' // demo case, would place inside .env file if real project! And would not expose in client-side code.
			}
		})
		const body = await response.text()

		if (!response.ok) {
			throw new Error(`Request failed (${response.status}): ${body || response.statusText}`)
		}

		return body
	}
}

const client = new HttpClient()
export default client;