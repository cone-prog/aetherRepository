import { Route, Routes } from 'react-router'
import FileProcessingPage from './pages/FileProcessingPage'
import Navbar from './components/Navbar'

function App() {

  return (
    <div className='bg-radial from-base-100 to-base-300 flex flex-col min-h-screen font-display'>
      <Navbar/>
      <Routes>
        <Route path="/" element={<FileProcessingPage/>}></Route>
      </Routes>
    </div>
  )
}

export default App
