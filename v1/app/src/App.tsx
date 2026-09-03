import { Routes, Route, useNavigate } from 'react-router-dom'
import './App.css'
import WeatherRoute from './routes/weather'
import StationRoute from './routes/station'
function App() {
	return (
		<Routes>
			<Route path="/" element={<WeatherLinks />} />
			<Route path="/weather/:weatherType" element={<WeatherRoute />} />
			<Route path="/weather/:weatherType/station/:id" element={<StationRoute />} />
		</Routes>
	)
}

function WeatherLinks() {
	const navigate = useNavigate()

	return (
		<ul>
			<li>
				<button onClick={() => navigate('/weather/1')}>Lufttemperatur</button>
			</li>
			<li>
				<button onClick={() => navigate('/weather/21')}>Byvind</button>
			</li>
		</ul>
	)
}

export default App
