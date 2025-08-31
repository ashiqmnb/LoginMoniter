import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import FingerprintJS from "@fingerprintjs/fingerprintjs";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { useState } from "react";

const Login = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const initialValues = { email: "", password: "" };

  const ipAddresses = [
    "20.110.53.72", // USA 0
    "49.44.24.1", // India 1
    "142.112.0.1", // Canada 2
    "203.0.178.191", // Australia 3
    "23.217.139.225", // Japan 4 blocked
    "185.156.174.1", // vpn 5
    "3.23.0.1" // proxies 6
  ];

  const deviceFingerprint = [
    "kp9v5hxumaf7cr7bq3",
    "t8i4u8hymaf7cr7bqt",
    "xzn2q8zsm7af7cr7by",
    "gqk4h2jpmaf7cr7bz2",
    "hzl0o1womaf7cr7c01"
  ];

  const validationSchema = Yup.object({
    email: Yup.string().email("Invalid email").required("Email is required"),
    password: Yup.string()
      .min(8, "Password must be at least 8 characters")
      .matches(/[A-Z]/, "Password must contain at least one uppercase letter")
      .matches(
        /[!@#$%^&*(),.?":{}|<>]/,
        "Password must contain at least one special character"
      )
      .required("Password required"),
  });

  const handleSubmit = async (values: typeof initialValues) => {
    try {
      setLoading(true); // start loading

      const fp = await FingerprintJS.load();
      const result = await fp.get();
      const fingerprint = result.visitorId;

      console.log("Fingerprint:", fingerprint);
      console.log("Email:", values.email);
      console.log("Password:", values.password);

      const response = await axios.post(
        "https://localhost:7017/api/Auth/login",
        {
          email: values.email,
          password: values.password,
          fingerprint: deviceFingerprint[1],
        },
        {
          headers: {
            "X-Forwarded-For": ipAddresses[3],
          },
        }
      );

      if (response.data.status == "Success") {
        console.log("Response success:", response.data);

        localStorage.setItem("email", values.email);
        localStorage.setItem("token", response.data.jwtToken);
        localStorage.setItem("role", response.data.role.toLowerCase());

        if (response.data.role === "Admin") {
          alert("Login successful, navigating to admin dashboard.");
          navigate("/admin");
        } else {
          alert("Login successful, navigating home page");
          navigate("/home");
        }
      } else if (response.data.status == "RequireCapcha") {
        console.log("Response RequireCapcha:", response.data);
        localStorage.setItem("userId", response.data.userId);
        alert("Please verify captcha to login");
        navigate("/captcha");
      } else if (response.data.status == "RequireOtp") {
        console.log("Response RequireOtp:", response.data);
        localStorage.setItem("userId", response.data.userId);
        alert("Please verify OTP sent to your email");
        navigate("/verify-otp");
      } else {
        console.log("response error:", response.data);
        alert(response.data.message);
      }
    } catch (err: unknown) {
      console.log("login error:", err);
      const errorMessage =
        err instanceof Error ? err.message : "An unknown error occurred";
      alert(errorMessage);
    } finally {
      setLoading(false); // stop loading
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="w-full max-w-sm bg-white p-6 rounded-2xl shadow-lg">
        <h2 className="text-2xl font-bold mb-4 text-center">Login</h2>
        <Formik
          initialValues={initialValues}
          validationSchema={validationSchema}
          onSubmit={handleSubmit}
        >
          <Form className="space-y-4">
            <div>
              <label className="block text-sm font-medium">Email</label>
              <Field
                type="email"
                name="email"
                className="w-full px-3 py-2 border rounded-lg focus:ring focus:ring-indigo-300"
              />
              <ErrorMessage
                name="email"
                component="div"
                className="text-red-500 text-sm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium">Password</label>
              <Field
                type="password"
                name="password"
                className="w-full px-3 py-2 border rounded-lg focus:ring focus:ring-indigo-300"
              />
              <ErrorMessage
                name="password"
                component="div"
                className="text-red-500 text-sm"
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className={`w-full py-2 rounded-lg text-white ${
                loading
                  ? "bg-indigo-400 cursor-not-allowed"
                  : "bg-indigo-600 hover:bg-indigo-700"
              }`}
            >
              {loading ? "Logging in..." : "Login"}
            </button>
          </Form>
        </Formik>
      </div>
    </div>
  );
};

export default Login;
