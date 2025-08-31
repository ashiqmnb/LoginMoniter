import React, { useEffect, useState } from "react";
import axios from "axios";
import Sidebar from "../components/Sidebar";

interface GeoLocationDTO {
  country: string;
  regionName: string;
  city: string;
  timezone: string;
  query: string;
}

const GeoLocationsPage: React.FC = () => {
  const [locations, setLocations] = useState<GeoLocationDTO[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const fetchLocations = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token");
      const response = await axios.get<GeoLocationDTO[]>(
        "https://localhost:7017/api/Admin/all-locations", // 🔹 API endpoint
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      setLocations(response.data);
    } catch (error) {
      console.error("Error fetching geo locations", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLocations();
  }, []);

  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <aside className="w-64 h-screen overflow-y-auto bg-gray-800 text-white">
        <Sidebar />
      </aside>

      {/* Main Content */}
      <div className="flex-1 overflow-y-auto p-6">
        <h1 className="text-2xl font-semibold mb-6">Geo Locations</h1>

        {loading ? (
          <p>Loading...</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full border-collapse border border-gray-300">
              <thead>
                <tr className="bg-gray-100">
                  <th className="border border-gray-300 px-4 py-2">#</th>
                  <th className="border border-gray-300 px-4 py-2">IP Address</th>
                  <th className="border border-gray-300 px-4 py-2">Country</th>
                  <th className="border border-gray-300 px-4 py-2">Region</th>
                  <th className="border border-gray-300 px-4 py-2">City</th>
                  <th className="border border-gray-300 px-4 py-2">Timezone</th>
                </tr>
              </thead>
              <tbody>
                {locations.length > 0 ? (
                  locations.map((loc, index) => (
                    <tr key={index} className="text-center">
                      <td className="border border-gray-300 px-4 py-2">{index + 1}</td>
                      <td className="border border-gray-300 px-4 py-2">{loc.query}</td>
                      <td className="border border-gray-300 px-4 py-2">{loc.country}</td>
                      <td className="border border-gray-300 px-4 py-2">{loc.regionName}</td>
                      <td className="border border-gray-300 px-4 py-2">{loc.city}</td>
                      <td className="border border-gray-300 px-4 py-2">{loc.timezone}</td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="text-center py-4 text-gray-500">
                      No geo locations found
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

export default GeoLocationsPage;
