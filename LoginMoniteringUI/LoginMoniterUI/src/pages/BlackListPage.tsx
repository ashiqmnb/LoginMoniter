import React, { useEffect, useState } from "react";
import axios from "axios";
import Sidebar from "../components/Sidebar";

interface BlackList {
  id: number;
  ipAddress: string;
  reason: string;
}

const BlackListPage: React.FC = () => {
  const [blackList, setBlackList] = useState<BlackList[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const fetchBlackList = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token");
      const response = await axios.get<BlackList[]>(
        "https://localhost:7017/api/Admin/blacklist",
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      setBlackList(response.data);
    } catch (error) {
      console.error("Error fetching black list", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBlackList();
  }, []);

  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
        <Sidebar />
      </aside>

      {/* Main Content */}
      <div className="flex-1 overflow-y-auto p-6">
        <div className="p-6">
          <h1 className="text-2xl font-semibold mb-6">Blacklisted IP Addresses</h1>

          {loading ? (
            <p>Loading...</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse border border-gray-300">
                <thead>
                  <tr className="bg-gray-100">
                    <th className="border border-gray-300 px-4 py-2">#</th>
                    <th className="border border-gray-300 px-4 py-2">IP Address</th>
                    <th className="border border-gray-300 px-4 py-2">Reasons</th>
                  </tr>
                </thead>
                <tbody>
                  {blackList.length > 0 ? (
                    blackList.map((entry, index) => {
                      // Split reasons if separated by semicolons
                      const reasonsList = entry.reason
                        .split(";")
                        .map((r) => r.trim());

                      return (
                        <tr key={entry.id} className="text-center">
                          <td className="border border-gray-300 px-4 py-2">{index + 1}</td>
                          <td className="border border-gray-300 px-4 py-2 font-semibold text-red-600">
                            {entry.ipAddress}
                          </td>
                          <td className="border border-gray-300 px-4 py-2 text-left">
                            <ul className="list-disc pl-5">
                              {reasonsList.map((reason, i) => (
                                <li key={i}>{reason}</li>
                              ))}
                            </ul>
                          </td>
                        </tr>
                      );
                    })
                  ) : (
                    <tr>
                      <td
                        colSpan={3}
                        className="text-center py-4 text-gray-500 border border-gray-300"
                      >
                        No blacklisted IPs found
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

export default BlackListPage;
