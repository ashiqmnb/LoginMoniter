import axios from "axios";
import React, { useState } from "react";
import ReCAPTCHA from "react-google-recaptcha";
import { useNavigate } from "react-router-dom";

const CaptchaForm: React.FC = () => {
  const [captchaToken, setCaptchaToken] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();

  const handleCaptcha = (token: string | null) => {
    setCaptchaToken(token);
  };

  const ipAddresses = [
    "20.110.53.72", // USA 0
    "49.44.24.1", // India 1
    "142.112.0.1", // Canada 2
    "203.0.178.191", // Australia 3
    "23.217.139.225", // Japan 4 blocked
    "185.156.174.1", // vpn 5
    "3.23.0.1" // proxies 6
  ];

  const handleSubmit = async () => {
    console.log("Submitting CAPTCHA");

    if (!captchaToken) {
      alert("Please verify CAPTCHA!");
      return;
    }

    try {
      setLoading(true);

      console.log("captcha token", captchaToken);

      const response = await axios.post(
        "https://localhost:7017/api/Risk/verify-capcha",
        {
          userId: localStorage.getItem("userId"),
          token: captchaToken,
        },
        {
          headers: {
            "X-Forwarded-For": ipAddresses[3],
          },
        }
      );

      const data = response.data;
      console.log("CAPTCHA Response:", data);

      if (data.status === "Success") {
        alert("Captcha verified successfully!");

        localStorage.clear();

        localStorage.setItem("email", data.email);
        localStorage.setItem("token", data.jwtToken);
        localStorage.setItem("role", data.role.toLowerCase());

        if (data.role === "Admin") {
          navigate("/admin");
        } else {
          navigate("/home");
        }
      }
      else if (response.data.status == "RequireOtp") {
        console.log("Response RequireOtp:", response.data);
        localStorage.setItem("userId", response.data.userId);
        alert("Please verify OTP sent to your email");
        navigate("/verify-otp");
      }
      else {
        alert("Failed to verify captcha.");
      }
    } catch (err) {
      console.error("Captcha verification error:", err);
      alert("Something went wrong. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center justify-center h-screen bg-gray-100">
      <div className="bg-white p-6 rounded-2xl shadow-md w-96 text-center">
        <h2 className="text-xl font-semibold mb-4">Captcha Verification</h2>

        <ReCAPTCHA
          sitekey={import.meta.env.VITE_RECAPTCHA_SITE_KEY}
          onChange={handleCaptcha}
          className="flex justify-center"
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

export default CaptchaForm;
