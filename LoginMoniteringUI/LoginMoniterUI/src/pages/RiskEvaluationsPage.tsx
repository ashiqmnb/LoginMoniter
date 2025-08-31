import React, { useEffect, useState } from "react";
import axios from "axios";
import Sidebar from "../components/Sidebar";

interface RiskEvaluation {
  id: number;
  score: number;
  category: string;
  reasons: string;
  evaluatedAt: string;
  email?: string | null;
  ipAddress?: string | null;
}

const RiskEvaluationsPage: React.FC = () => {
  const [evaluations, setEvaluations] = useState<RiskEvaluation[]>([]);
  const [filter, setFilter] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(false);

  const fetchEvaluations = async (riskLevel: string) => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token");
      const queryParam = riskLevel !== "" ? `?riskLevel=${riskLevel}` : "";
      const response = await axios.get<RiskEvaluation[]>(
        `https://localhost:7017/api/Admin/risk-evaluations${queryParam}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      setEvaluations(response.data);
    } catch (error) {
      console.error("Error fetching risk evaluations", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEvaluations(filter);
  }, [filter]);

  const getRowColor = (category: string) => {
    switch (category.toLowerCase()) {
      case "low risk":
        return "bg-green-200";
      case "medium risk":
        return "bg-yellow-200";
      case "high risk":
        return "bg-red-200";
      default:
        return "";
    }
  };

  return (
    <div className="flex h-screen">
      <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
        <Sidebar />
      </aside>

      <div className="flex-1 overflow-y-auto p-6">
        <div className="p-6">
          <div className="flex justify-between items-center mb-4">
            <h1 className="text-2xl font-semibold mb-4">Risk Evaluations</h1>

            <div className="mb-4">
              <label className="mr-2 font-medium">Filter by Risk Level:</label>
              <select
                className="border px-3 py-2 rounded"
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
              >
                <option value="">All</option>
                <option value="Low Risk">Low Risk</option>
                <option value="Medium Risk">Medium Risk</option>
                <option value="High Risk">High Risk</option>
              </select>
            </div>
          </div>

          {loading ? (
            <p>Loading...</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse border border-gray-300">
                <thead>
                  <tr className="bg-gray-100">
                    <th className="border border-gray-300 px-4 py-2">#</th>
                    <th className="border border-gray-300 px-4 py-2">Email</th>
                    <th className="border border-gray-300 px-4 py-2">Score</th>
                    <th className="border border-gray-300 px-4 py-2">Category</th>
                    <th className="border border-gray-300 px-4 py-2">Reasons</th>
                    <th className="border border-gray-300 px-4 py-2">IP Address</th>
                    <th className="border border-gray-300 px-4 py-2">Date & Time</th>
                  </tr>
                </thead>
                <tbody>
                    {evaluations.length > 0 ? (
                        evaluations.map((evalItem, index) => {
                        const dateObj = new Date(evalItem.evaluatedAt);
                        const date = dateObj.toLocaleDateString();
                        const time = dateObj.toLocaleTimeString();
                        const reasonsList = evalItem.reasons.split(";").map((r) => r.trim());

                        return (
                            <tr key={index} className={getRowColor(evalItem.category)}>
                            <td className="border border-gray-300 px-4 py-2">{index + 1}</td>
                            <td className="border border-gray-300 px-4 py-2">{evalItem.email ?? "Unknown"}</td>
                            <td className="border border-gray-300 px-4 py-2">{evalItem.score}</td>
                            <td className="border border-gray-300 px-4 py-2">{evalItem.category}</td>
                            <td className="border border-gray-300 px-4 py-2">
                                <ul className="list-disc pl-5">
                                {reasonsList.map((reason, i) => (
                                    <li key={i}>{reason}</li>
                                ))}
                                </ul>
                            </td>
                            <td className="border border-gray-300 px-4 py-2">{evalItem.ipAddress ?? "Unknown"}</td>
                            <td className="border border-gray-300 px-4 py-2">
                                <div>
                                {date}<br />{time}
                                </div>
                            </td>
                            </tr>
                        );
                        })
                    ) : (
                        <tr>
                        <td colSpan={7} className="text-center py-4 text-gray-500 border border-gray-300">
                            No risk evaluations found
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

export default RiskEvaluationsPage;
