// src/components/ChatBot.tsx
import axios from "axios";
import React, { useState } from "react";

type Message = {
  text: string;
  sender: "user" | "bot";
};

const ChatBot: React.FC = () => {
  const [question, setQuestion] = useState("");
  const [messages, setMessages] = useState<Message[]>([]);

  const askQuestion = async () => {
    if (!question.trim()) return;

    setMessages((prev) => [...prev, { text: question, sender: "user" }]);

    try {
      const response = await axios.post("http://localhost:5045/api/chatbot", {
        question,
      });

      setMessages((prev) => [
        ...prev,
        { text: response.data.answer, sender: "bot" },
      ]);
    } catch (error) {
      setMessages((prev) => [
        ...prev,
        { text: "Erreur de connexion au chatbot.", sender: "bot" },
      ]);
    }

    setQuestion("");
  };

  return (
    <div style={styles.container}>
      <h2 style={styles.header}>🛒 Chatbot e-commerce</h2>

      <div style={styles.chatBox}>
        {messages.map((msg, index) => (
          <div
            key={index}
            style={{
              ...styles.message,
              alignSelf: msg.sender === "user" ? "flex-end" : "flex-start",
              backgroundColor: msg.sender === "user" ? "#d1e7dd" : "#f8d7da",
            }}
          >
            <strong>{msg.sender === "user" ? "Vous" : "Bot"}:</strong>{" "}
            {msg.text}
          </div>
        ))}
      </div>

      <div style={styles.inputArea}>
        <input
          type="text"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && askQuestion()}
          placeholder="Posez une question..."
          style={styles.input}
        />
        <button onClick={askQuestion} style={styles.button}>
          Envoyer
        </button>
      </div>
    </div>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  container: {
    width: "400px",
    margin: "40px auto",
    padding: "20px",
    border: "1px solid #ccc",
    borderRadius: "12px",
    boxShadow: "0 2px 10px rgba(0,0,0,0.1)",
    fontFamily: "Arial, sans-serif",
    backgroundColor: "#fff",
  },
  header: {
    textAlign: "center",
    marginBottom: "15px",
  },
  chatBox: {
    display: "flex",
    flexDirection: "column",
    gap: "10px",
    height: "300px",
    overflowY: "auto",
    border: "1px solid #ddd",
    borderRadius: "8px",
    padding: "10px",
    backgroundColor: "#f9f9f9",
  },
  message: {
    maxWidth: "80%",
    padding: "8px 12px",
    borderRadius: "8px",
  },
  inputArea: {
    marginTop: "10px",
    display: "flex",
    gap: "8px",
  },
  input: {
    flex: 1,
    padding: "10px",
    borderRadius: "6px",
    border: "1px solid #ccc",
  },
  button: {
    padding: "10px 14px",
    borderRadius: "6px",
    border: "none",
    backgroundColor: "#007bff",
    color: "#fff",
    cursor: "pointer",
  },
};

export default ChatBot;
