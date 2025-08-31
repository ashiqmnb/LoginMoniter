import { useNavigate } from "react-router-dom";

export default function HomePage() {
  const navigate = useNavigate();
  const email = localStorage.getItem("email");

  const handleLogout = () => {
    localStorage.clear();
    navigate("/");
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100 p-6">
      <div className="bg-white shadow-lg rounded-2xl p-8 w-full max-w-md text-center">
        <h2 className="text-2xl font-bold text-gray-800 mb-4">
          Welcome to Home Page
        </h2>
        <p className="text-gray-600 mb-6">
          <span className="font-semibold">User:</span> {email}
        </p>
        <button
          onClick={handleLogout}
          className="w-full bg-purple-600 hover:bg-purple-700 text-white font-semibold py-2 px-4 rounded-lg transition duration-200"
        >
          Logout
        </button>
      </div>
    </div>
  );
}
