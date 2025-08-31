import { Link, useNavigate } from "react-router-dom";
import { FaTachometerAlt, FaUsers, FaSignInAlt, FaExclamationTriangle, FaSignOutAlt } from "react-icons/fa";
import { FaMapLocation } from "react-icons/fa6";
import { RiSkullFill } from "react-icons/ri";


const Sidebar = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.clear();
    navigate("/");
  };

  return (
    <div className="h-screen w-64 bg-gradient-to-b from-indigo-700 to-indigo-800 text-white flex flex-col p-6 shadow-lg pt-16 ">
      <h2 className="text-3xl font-bold mb-8 text-center tracking-wide">Admin Panel</h2>
      <nav className="flex flex-col gap-4 mt-10">
        <Link to="/admin"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <FaTachometerAlt /> <span>Dashboard</span>
        </Link>
        <Link to="/admin/risk-levels"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <FaTachometerAlt /> <span>Risk Levels</span>
        </Link>

        <Link to="/admin/user-status"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <FaUsers /> <span>User Status</span>
        </Link>

        <Link to="/admin/login-attempts"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <FaSignInAlt /> <span>Login Attempts</span>
        </Link>

        <Link to="/admin/risk-evaluations"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <FaExclamationTriangle /> <span>Risk Evaluations</span>
        </Link>

        <Link to="/admin/blacklist"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <RiSkullFill /> <span>Blacklisted IPs</span>
        </Link>

        <Link to="/admin/all-locations"
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-indigo-600 transition-all duration-200 shadow-sm"
        >
          <FaMapLocation /> <span>All Locations</span>
        </Link>

        <button onClick={handleLogout}
          className="flex items-center gap-3 p-3 rounded-lg hover:bg-red-600 transition-all duration-200 shadow-sm mt-6"
        >
          <FaSignOutAlt /> Logout
        </button>
      </nav>
    </div>
  );
};

export default Sidebar;
