import axios from "axios";

const apiClient = axios.create({
  baseURL: "https://localhost:7135", // Replace with your API URL
  withCredentials: true, // Send cookies with requests
});

export default apiClient;
