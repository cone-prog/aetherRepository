import { Sparkles } from "lucide-react"
import { NavLink } from "react-router"


const Navbar = () => {

  return (
    <div className="navbar bg-base-100 shadow-xs">
      <NavLink
        to="/"
        className="btn btn-ghost text-primary text-2xl font-bold select-none"
        viewTransition>
          <Sparkles/>Aether
      </NavLink>
    </div>
  )
}

export default Navbar