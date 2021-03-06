import { useEffect } from "react"
import { useLocation, useNavigate } from "react-router-dom"
import { IoArrowBack, IoHome } from "react-icons/io5"
import Button from "asmr/components/Button"
import useLogger from "asmr/libs/hooks/loggerHook"
import ErrorPage from "asmr/pages/Errors/ErrorPage"

function NotFoundPage(): JSX.Element {
	const location = useLocation()
	const logger = useLogger(NotFoundPage)
	const navigate = useNavigate()

	function onBackButtonClicked() {
		navigate(-1)
	}

	function onHomePageButtonClicked() {
		navigate("/")
	}

	useEffect(() => {
		logger.info("Page not found, location:", location.pathname + location.search)
	}, [])

	return (
		<ErrorPage
			documentTitle="Not Found"
			title="Oops!"
			message="The page you are looking for is not found."
			clickToActions={
				<>
					<Button icon={IoArrowBack} onClick={onBackButtonClicked}>
						Back
					</Button>
					&nbsp;
					<Button icon={IoHome} onClick={onHomePageButtonClicked}>
						Home Page
					</Button>
				</>
			}
		/>
	)
}

export default NotFoundPage
