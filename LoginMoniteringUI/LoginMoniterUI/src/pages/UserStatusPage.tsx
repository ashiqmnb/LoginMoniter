import React, { useEffect, useState } from "react";
import axios from "axios";
import Sidebar from "../components/Sidebar";

interface UserStatus {
  userId: string;
  username: string;
  email: string;
  successCount: number;
  failedCount: number;
  blockedCount: number;
}

const UserStatusPage: React.FC = () => {
  const [users, setUsers] = useState<UserStatus[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const fetchUserStatus = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token");
      const response = await axios.get<UserStatus[]>(
        "https://localhost:7017/api/Admin/login-stats", // 👈 your API
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      setUsers(response.data);
    } catch (error) {
      console.error("Error fetching user status", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUserStatus();
  }, []);

  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
        <Sidebar />
      </aside>

      {/* Main Content */}
      <div className="flex-1 overflow-y-auto p-12">
        <h1 className="text-2xl font-semibold mb-6">User Status</h1>

        {loading ? (
          <p>Loading...</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full border-collapse border border-gray-300">
              <thead>
                <tr className="bg-gray-100">
                  <th className="border border-gray-300 px-4 py-2">#</th>
                  <th className="border border-gray-300 px-4 py-2">Username</th>
                  <th className="border border-gray-300 px-4 py-2">Email</th>
                  <th className="border border-gray-300 px-4 py-2">Success</th>
                  <th className="border border-gray-300 px-4 py-2">Failed</th>
                  <th className="border border-gray-300 px-4 py-2">Blocked</th>
                  <th className="border border-gray-300 px-4 py-2">Total Attempts</th>
                </tr>
              </thead>
              <tbody>
                {users.length > 0 ? (
                  users.map((user, index) => (
                    <tr key={user.userId} className="text-center">
                      <td className="border border-gray-300 px-4 py-2">{index + 1}</td>
                      <td className="border border-gray-300 px-4 py-2">{user.username}</td>
                      <td className="border border-gray-300 px-4 py-2">{user.email}</td>
                      <td className="border border-gray-300 px-4 py-2 text-green-600 font-semibold">
                        {user.successCount}
                      </td>
                      <td className="border border-gray-300 px-4 py-2 text-red-600 font-semibold">
                        {user.failedCount}
                      </td>
                      <td className="border border-gray-300 px-4 py-2 text-gray-600 font-semibold">
                        {user.blockedCount}
                      </td>
                      <td className="border border-gray-300 px-4 py-2 font-bold">
                        {user.successCount + user.failedCount + user.blockedCount}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="text-center py-4 text-gray-500">
                      No user status data found
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default UserStatusPage;
