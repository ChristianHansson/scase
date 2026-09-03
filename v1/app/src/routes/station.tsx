import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import httpClient from '../services/http-client'

function StationRoute() {
	const { id, weatherType } = useParams()
	const [data, setData] = useState<string | null>(null)
	const [error, setError] = useState<string | null>(null)
	const [period, setPeriod] = useState<string>("latest-hour");

	useEffect(() => {
		if (!weatherType || !id) {
			return
		}

		httpClient
			.get(`/weather/${weatherType}/stations/${id}?period=${period}`)
			.then(setData)
			.catch((requestError: unknown) => {
				setError(
					requestError instanceof Error
						? requestError.message
						: 'Unable to load station.',
				)
			})
	}, [weatherType, id, period])

	return (
		<>
			<h1>Station {id}</h1>
			<p>Weather type: {weatherType}</p>
			<button onClick={() => setPeriod("latest-hour")}>Latest Hour</button>
			<button onClick={() => setPeriod("latest-day")}>Latest Day</button>
			<Link to={`/weather/${weatherType}`}>Back to stations</Link>
			{error && <p role="alert">{error}</p>}
			{data && <pre>{data}</pre>}
		</>
	)
}

export default StationRoute