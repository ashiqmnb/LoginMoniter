import axios from "axios";
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

const OtpPage: React.FC = () => {
  const [otp, setOtp] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();

  const ipAddresses = [
    "20.110.53.72", // USA 0
    "49.44.24.1", // India 1
    "142.112.0.1", // Canada 2
    "203.0.178.191", // Australia 3
    "23.217.139.225", // Japan 4 blocked
    "185.156.174.1", // vpn 5
    "3.23.0.1" // proxies 6
  ];

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;

    // ✅ Allow only digits and max 6 characters
    if (/^\d{0,6}$/.test(value)) {
      setOtp(value);
    }
  };

  const handleSubmit = async () => {
    if (otp.length !== 6) {
      alert("OTP must be 6 digits");
      return;
    }

    try {
      setLoading(true);

      const otpString: string = otp.toString();

      const response = await axios.patch("https://localhost:7017/api/Auth/verifyOtp", {
          otp: otpString,
          userId: localStorage.getItem("userId"),
        },
        {
          headers: {
            "X-Forwarded-For": ipAddresses[3],
          },
        }
      );

      if (response.data.status === "Success") {
        alert("OTP verified successfully!");

        localStorage.clear();
        localStorage.setItem("email", response.data.email);
        localStorage.setItem("token", response.data.jwtToken);
        localStorage.setItem("role", response.data.role.toLowerCase());

        if (response.data.role === "Admin") {
          navigate("/admin");
        } else {
          navigate("/home");
        }
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      console.error("OTP verification error:", err);
      alert("Something went wrong. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gray-100">
      <div className="bg-white p-6 rounded-2xl shadow-md w-80">
        <h2 className="text-xl font-semibold mb-4 text-center">Enter OTP</h2>
        
        <input
          type="text"
          value={otp}
          onChange={handleChange}
          maxLength={6}
          className="w-full px-4 py-2 border rounded-lg text-center tracking-widest text-lg"
          placeholder="Enter 6-digit OTP"
        />

        <button
          onClick={handleSubmit}
          disabled={loading}
          className={`mt-4 w-full py-2 rounded-lg text-white transition ${
            loading
              ? "bg-blue-400 cursor-not-allowed"
              : "bg-blue-600 hover:bg-blue-700"
          }`}
        >
          {loading ? "Verifying..." : "Submit"}
        </button>
      </div>
    </div>
  );
};

export default OtpPage;
