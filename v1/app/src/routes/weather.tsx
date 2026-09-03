import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import httpClient from '../services/http-client'

type Station = {
	id: number
	key: string
	title: string
	name: string
}

type StationsResponse = {
	stations: Station[]
}

type Statistics = {
	count: number
	sum: number
	mean: number
	median: number
	min: number
	max: number
	stdDev: number
}

function WeatherRoute() {
	const navigate = useNavigate()
	const { weatherType } = useParams()
	const [stations, setStations] = useState<Station[] | null>(null)
	const [stationsError, setStationsError] = useState<string | null>(null)
	const [isLoadingStations, setIsLoadingStations] = useState(false)

	const [statistics, setStatistics] = useState<Statistics | null>(null)
	const [statisticsError, setStatisticsError] = useState<string | null>(null)
	const [isLoadingStatistics, setIsLoadingStatistics] = useState(false)

	async function getStations() {
		if (!weatherType) {
			return
		}

		setIsLoadingStations(true)
		setStationsError(null)

		try {
			const data = await httpClient.get(`/weather/${weatherType}/stations`)
			const response = JSON.parse(data) as StationsResponse
			setStations(response.stations ?? [])
		} catch (requestError: unknown) {
			setStationsError(
				requestError instanceof Error
					? requestError.message
					: 'Unable to load stations.',
			)
		} finally {
			setIsLoadingStations(false)
		}
	}

	async function getStatistics() {
		if (!weatherType) {
			return
		}

		setIsLoadingStatistics(true)
		setStatisticsError(null)

		try {
			const data = await httpClient.get(`/weather/${weatherType}`)
			setStatistics(JSON.parse(data) as Statistics)
		} catch (requestError: unknown) {
			setStatisticsError(
				requestError instanceof Error
					? requestError.message
					: 'Unable to load statistics.',
			)
		} finally {
			setIsLoadingStatistics(false)
		}
	}

	return (
		<>
			<h1>Weather type {weatherType == "1" ? 'Lufttemperatur' : 'Byvind'}</h1>

			<button type="button" onClick={getStatistics} disabled={isLoadingStatistics}>
				{isLoadingStatistics ? 'Loading statistics...' : 'View statistics'}
			</button>
			{statisticsError && <p role="alert">{statisticsError}</p>}
			{statistics && (
				<ul>
					<li>Count: {statistics.count}</li>
					<li>Sum: {statistics.sum}</li>
					<li>Mean: {statistics.mean}</li>
					<li>Median: {statistics.median}</li>
					<li>Min: {statistics.min}</li>
					<li>Max: {statistics.max}</li>
					<li>Std dev: {statistics.stdDev}</li>
				</ul>
			)}

			<button type="button" onClick={getStations} disabled={isLoadingStations}>
				{isLoadingStations ? 'Loading stations...' : 'View all stations'}
			</button>
			{stationsError && <p role="alert">{stationsError}</p>}
			{stations && stations.length === 0 && <p>No stations found.</p>}
			{stations && stations.length > 0 && (
				<ul>
					{stations.map((station) => (
						<li key={station.id}>
							<button
								type="button"
								onClick={() =>
									navigate(`/weather/${weatherType}/station/${station.id}`)
								}
							>
								<strong>{station.title}</strong>
								<span>{station.name}</span>
								<span>{station.key}</span>
							</button>
						</li>
					))}
				</ul>
			)}
		</>
	)
}

export default WeatherRoute
