import { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import axios from "axios";

interface RiskSetting {
  id: number;
  lowRiskMax: number;
  mediumRiskMax: number;
  maxFailedAttempts: number;
  timeDuration: number;
  isActive: boolean;
}

const AdminDashboard = () => {
  const [riskSettings, setRiskSettings] = useState<RiskSetting[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [mode, setMode] = useState<"add" | "edit">("add");
  const [selectedId, setSelectedId] = useState<number | null>(null);

  const [formData, setFormData] = useState({
    lowRiskMax: 0,
    mediumRiskMax: 0,
    maxFailedAttempts: 0,
    timeDuration: 0,
  });

  const token = localStorage.getItem("token");

  const fetchRiskSettings = async () => {
    try {
      const res = await axios.get<RiskSetting[]>(
        "https://localhost:7017/api/Admin/get-risk-settings",
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      setRiskSettings(res.data);
    } catch (error) {
      console.error("Error fetching risk settings", error);
    }
  };

  useEffect(() => {
    fetchRiskSettings();
  }, []);

  const handleActivate = async (id: number) => {
    try {
      await axios.patch(
        `https://localhost:7017/api/Admin/switch-active/${id}`,
        {},
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      fetchRiskSettings();
    } catch (error) {
      console.error("Error activating risk setting", error);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await axios.delete(
        `https://localhost:7017/api/Admin/delete-risk-settings/${id}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      fetchRiskSettings();
    } catch (error) {
      console.error("Error deleting risk setting", error);
    }
  };

  const handleAddOrUpdate = async () => {
    try {
      if (mode === "add") {
        const res = await axios.post(
          "https://localhost:7017/api/Admin/risk-settings",
          formData,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        alert(res.data.message);
      } else if (mode === "edit" && selectedId !== null) {
        const res = await axios.put(
          `https://localhost:7017/api/Admin/risk-settings/${selectedId}`,
          formData,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        alert(res.data.message);
      }

      setShowModal(false);
      setFormData({
        lowRiskMax: 0,
        mediumRiskMax: 0,
        maxFailedAttempts: 0,
        timeDuration: 0,
      });
      setSelectedId(null);
      setMode("add");
      fetchRiskSettings();
    } catch (error: unknown) {
      if (
        axios.isAxiosError(error) &&
        error.response &&
        error.response.data &&
        error.response.data.message
      ) {
        alert(error.response.data.message);
      } else {
        alert("An error occurred while saving risk setting.");
      }
    }
  };

  const handleEdit = (rs: RiskSetting) => {
    setMode("edit");
    setSelectedId(rs.id);
    setFormData({
      lowRiskMax: rs.lowRiskMax,
      mediumRiskMax: rs.mediumRiskMax,
      maxFailedAttempts: rs.maxFailedAttempts,
      timeDuration: rs.timeDuration,
    });
    setShowModal(true);
  };

  return (
    <div className="flex h-screen ">
      <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
        <Sidebar />
      </aside>

      {/* Main content */}
      <div className="flex-1 overflow-y-auto p-6">
        <div className="flex justify-between items-center p-6">
          <h1 className="text-2xl font-semibold mb-4">Login Attempts</h1>
          <button
            onClick={() => {
              setMode("add");
              setFormData({
                lowRiskMax: 0,
                mediumRiskMax: 0,
                maxFailedAttempts: 0,
                timeDuration: 0,
              });
              setShowModal(true);
            }}
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
          >
            Add New Settings
          </button>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full border-collapse border border-gray-300">
              <thead>
                <tr className="bg-gray-100">
                  <th className="px-4 py-2 border">#</th>
                  <th className="px-4 py-2 border">Low Risk Max</th>
                  <th className="px-4 py-2 border">Medium Risk Max</th>
                  <th className="px-4 py-2 border">Max Failed Attempts</th>
                  <th className="px-4 py-2 border">Time Duration (min)</th>
                  <th className="px-4 py-2 border">Status</th>
                  <th className="px-4 py-2 border">Actions</th>
                </tr>
              </thead>
              <tbody>
                {riskSettings.map((rs, index) => (
                  <tr
                    key={rs.id}
                    className={`text-center ${rs.isActive ? "bg-green-200" : ""}`}
                  >
                    <td className="px-4 py-2 border">{index + 1}</td>
                    <td className="px-4 py-2 border">{rs.lowRiskMax}</td>
                    <td className="px-4 py-2 border">{rs.mediumRiskMax}</td>
                    <td className="px-4 py-2 border">{rs.maxFailedAttempts}</td>
                    <td className="px-4 py-2 border">{rs.timeDuration}</td>
                    <td className="px-4 py-2 border">
                      {rs.isActive ? (
                        <button className="bg-green-500 text-white px-3 py-1 rounded">
                          In Use
                        </button>
                      ) : (
                        <button
                          onClick={() => handleActivate(rs.id)}
                          className="bg-blue-500 text-white px-3 py-1 rounded hover:bg-blue-600"
                        >
                          Activate
                        </button>
                      )}
                    </td>
                    <td className="px-4 py-2 border space-x-2">
                      <button
                        onClick={() => handleEdit(rs)}
                        className="bg-yellow-500 text-white px-3 py-1 rounded hover:bg-yellow-600"
                      >
                        Update
                      </button>
                      {!rs.isActive && (
                        <button
                          onClick={() => handleDelete(rs.id)}
                          className="bg-red-500 text-white px-3 py-1 rounded hover:bg-red-600"
                        >
                          Delete
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
        </div>
        

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-black/30 backdrop-blur-xs flex items-center justify-center z-50">

            <div className="bg-white rounded-lg shadow-lg p-6 w-96">
              <h2 className="text-xl font-bold mb-4">
                {mode === "add" ? "Add Risk Setting" : "Update Risk Setting"}
              </h2>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">
                    Low Risk Max
                  </label>
                  <input
                    type="number"
                    placeholder="Low Risk Max"
                    value={formData.lowRiskMax}
                    onChange={(e) =>
                      setFormData({ ...formData, lowRiskMax: +e.target.value })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Medium Risk Max
                  </label>
                  <input
                    type="number"
                    placeholder="Medium Risk Max"
                    value={formData.mediumRiskMax}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        mediumRiskMax: +e.target.value,
                      })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Max Failed Attempts
                  </label>
                  <input
                    type="number"
                    placeholder="Max Failed Attempts"
                    value={formData.maxFailedAttempts}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        maxFailedAttempts: +e.target.value,
                      })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Time Duration (min)
                  </label>
                  <input
                    type="number"
                    placeholder="Time Duration (min)"
                    value={formData.timeDuration}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        timeDuration: +e.target.value,
                      })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 mt-6">
                <button
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 bg-gray-300 rounded hover:bg-gray-400"
                >
                  Cancel
                </button>
                <button
                  onClick={handleAddOrUpdate}
                  className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
                >
                  {mode === "add" ? "Save" : "Update"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminDashboard;
