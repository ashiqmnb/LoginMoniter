import { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import axios from "axios";

interface RiskLevel {
  id: number;
  name: string;
  minScore: number;
  maxScore: number;
  actions: string[];
}

interface FormData {
  name: string;
  minScore: number;
  maxScore: number;
  actions: string;
}

const RiskLevelsPage = () => {
  const [riskLevels, setRiskLevels] = useState<RiskLevel[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [mode, setMode] = useState<"add" | "edit">("add");
  const [selectedId, setSelectedId] = useState<number | null>(null);

  const [formData, setFormData] = useState<FormData>({
    name: "",
    minScore: 0,
    maxScore: 0,
    actions: "",
  });

  const token = localStorage.getItem("token");
  const apiUrl = "https://localhost:7017/api/Risk";

  const fetchRiskLevels = async () => {
    try {
      const res = await axios.get<RiskLevel[]>(apiUrl, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setRiskLevels(res.data);
    } catch (error) {
      console.error("Error fetching risk levels", error);
    }
  };

  useEffect(() => {
    fetchRiskLevels();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await axios.delete(`${apiUrl}/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchRiskLevels();
    } catch (error) {
      console.error("Error deleting risk level", error);
    }
  };

  const handleAddOrUpdate = async () => {
    try {
      if (mode === "add") {
        await axios.post(
          apiUrl,
          {
            ...formData,
            actions: formData.actions.split(",").map((a) => a.trim()),
          },
          { headers: { Authorization: `Bearer ${token}` } }
        );
      } else if (mode === "edit" && selectedId !== null) {
        await axios.put(
          `${apiUrl}/${selectedId}`,
          {
            ...formData,
            actions: formData.actions.split(",").map((a) => a.trim()),
          },
          { headers: { Authorization: `Bearer ${token}` } }
        );
      }

      setShowModal(false);
      setFormData({ name: "", minScore: 0, maxScore: 0, actions: "" });
      setSelectedId(null);
      setMode("add");
      fetchRiskLevels();
    } catch (error: unknown) {
      alert("An error occurred while saving risk level." + (error ? ` ${error}` : ""));
    }
  };

  const handleEdit = (rl: RiskLevel) => {
    setMode("edit");
    setSelectedId(rl.id);
    setFormData({
      name: rl.name,
      minScore: rl.minScore,
      maxScore: rl.maxScore,
      actions: rl.actions.join(", "),
    });
    setShowModal(true);
  };

  return (
    <div className="flex h-screen">
      <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
        <Sidebar />
      </aside>

      <div className="flex-1 overflow-y-auto p-6">
        <div className="flex justify-between items-center p-6">
          <h1 className="text-2xl font-semibold mb-4">Risk Levels</h1>
          <button
            onClick={() => {
              setMode("add");
              setFormData({ name: "", minScore: 0, maxScore: 0, actions: "DirectLogin" });
              setShowModal(true);
            }}
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
          >
            Add Risk Level
          </button>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full border-collapse border border-gray-300">
            <thead>
              <tr className="bg-gray-100">
                <th className="px-4 py-2 border">#</th>
                <th className="px-4 py-2 border">Name</th>
                <th className="px-4 py-2 border">Min Score</th>
                <th className="px-4 py-2 border">Max Score</th>
                <th className="px-4 py-2 border">Actions</th>
                <th className="px-4 py-2 border">Manage</th>
              </tr>
            </thead>
            <tbody>
              {riskLevels.map((rl, index) => (
                <tr key={rl.id} className="text-center">
                  <td className="px-4 py-2 border">{index + 1}</td>
                  <td className="px-4 py-2 border">{rl.name}</td>
                  <td className="px-4 py-2 border">{rl.minScore}</td>
                  <td className="px-4 py-2 border">{rl.maxScore}</td>
                  <td className="px-4 py-2 border">
                    {rl.actions && rl.actions.length > 0
                      ? rl.actions.join(", ")
                      : "—"}
                  </td>
                  <td className="px-4 py-2 border space-x-2">
                    <button
                      onClick={() => handleEdit(rl)}
                      className="bg-yellow-500 text-white px-3 py-1 rounded hover:bg-yellow-600"
                    >
                      Update
                    </button>
                    <button
                      onClick={() => handleDelete(rl.id)}
                      className="bg-red-500 text-white px-3 py-1 rounded hover:bg-red-600"
                    >
                      Delete
                    </button>
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
                {mode === "add" ? "Add Risk Level" : "Update Risk Level"}
              </h2>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">Name</label>
                  <input
                    type="text"
                    placeholder="Risk Level Name"
                    value={formData.name}
                    onChange={(e) =>
                      setFormData({ ...formData, name: e.target.value })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Min Score
                  </label>
                  <input
                    type="number"
                    placeholder="Min Score"
                    value={formData.minScore}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        minScore: +e.target.value,
                      })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Max Score
                  </label>
                  <input
                    type="number"
                    placeholder="Max Score"
                    value={formData.maxScore}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        maxScore: +e.target.value,
                      })
                    }
                    className="w-full border px-3 py-2 rounded"
                  />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Actions</label>
                    <select
                        value={formData.actions}
                        onChange={(e) =>
                        setFormData({ ...formData, actions: e.target.value })
                        }
                        className="w-full border px-3 py-2 rounded"
                    >
                        <option value="DirectLogin">DirectLogin</option>
                        <option value="Capcha">Capcha</option>
                        <option value="Otp">Otp</option>
                        <option value="Capcha, Otp">Capcha & Otp</option>
                        <option value="Block">Block</option>
                    </select>
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

export default RiskLevelsPage;
