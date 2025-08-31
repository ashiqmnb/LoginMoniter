import React, { useEffect, useState } from "react";
import axios from "axios";
import Sidebar from "../components/Sidebar";

interface LoginAttempt {
  id: number;
  userId: string;
  email: string;
  ipAddress: string;
  userAgent: string;
  deviceFingerprint: string;
  attemptedAt: string;
  result: string;
}

const LoginAttemptsPage: React.FC = () => {
  const [attempts, setAttempts] = useState<LoginAttempt[]>([]);
  const [filter, setFilter] = useState<string>("all");
  const [loading, setLoading] = useState<boolean>(false);

  const fetchAttempts = async (result: string) => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token");
      const response = await axios.get<LoginAttempt[]>(
        `https://localhost:7017/api/Admin/GetAllLoginAttempts?result=${result}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
    //   console.log("attempts", response.data);
      setAttempts(response.data);
    } catch (error) {
      console.error("Error fetching login attempts", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAttempts(filter);
  }, [filter]);

  // ✅ Row background color based on result
  const getRowColor = (result: string) => {
    switch (result.toLowerCase()) {
      case "success":
        return "bg-green-200"; // light green
      case "failed":
        return "bg-red-200"; // light red
      case "blocked":
        return "bg-gray-400"; // light gray
      case "requireotp":
        return "bg-yellow-200"; // light yellow
      default:
        return "";
    }
  };

  return (
    <div className="flex h-screen">
        {/* Sidebar fixed height, scrolls independently if needed */}
        <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
            <Sidebar />
        </aside>

        {/* Main content scrolls */}
        <div className="flex-1 overflow-y-auto p-6">
            <div className="p-6">
            <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold mb-4">Login Attempts</h1>

                {/* Dropdown filter */}
                <div className="mb-4">
                <label className="mr-2 font-medium">Filter by Result:</label>
                <select
                    className="border px-3 py-2 rounded"
                    value={filter}
                    onChange={(e) => setFilter(e.target.value)}
                >
                    <option value="all">All</option>
                    <option value="success">Success</option>
                    <option value="failed">Failed</option>
                    <option value="blocked">Blocked</option>
                    <option value="requireotp">Required OTP</option>
                </select>
                </div>
            </div>

            {/* Table */}
            {loading ? (
                <p>Loading...</p>
            ) : (
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse border border-gray-300">
                      <thead>
                      <tr className="bg-gray-100">
                          <th className="border border-gray-300 px-4 py-2">#</th>
                          <th className="border border-gray-300 px-4 py-2">Email</th>
                          <th className="border border-gray-300 px-4 py-2">IP Address</th>
                          <th className="border border-gray-300 px-4 py-2">Date</th>
                          <th className="border border-gray-300 px-4 py-2">Time</th>
                          <th className="border border-gray-300 px-4 py-2">Result</th>
                      </tr>
                      </thead>
                      <tbody>
                      {attempts.length > 0 ? (
                          attempts.map((attempt, index) => {
                          const dateObj = new Date(attempt.attemptedAt);
                          const date = dateObj.toLocaleDateString();
                          const time = dateObj.toLocaleTimeString();

                          return (
                              <tr key={index} className={getRowColor(attempt.result)}>
                                <td className="border border-gray-300 px-4 py-2">{index + 1}</td>
                                <td className="border border-gray-300 px-4 py-2">{attempt.email}</td>
                                <td className="border border-gray-300 px-4 py-2">{attempt.ipAddress ?? "Unknown"}</td>
                                <td className="border border-gray-300 px-4 py-2">{date}</td>
                                <td className="border border-gray-300 px-4 py-2">{time}</td>
                                <td className="border border-gray-300 px-4 py-2 capitalize">{attempt.result}</td>
                              </tr>
                          );
                          })
                      ) : (
                          <tr>
                          <td colSpan={6} className="text-center py-4 text-gray-500 border border-gray-300">
                              No login attempts found
                          </td>
                          </tr>
                      )}
                      </tbody>
                  </table>
                </div>
            )}
            </div>
        </div>
        </div>

  );
};

export default LoginAttemptsPage;
