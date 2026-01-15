import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

// NOTE: Replace with your actual backend URL.
const HUB_URL = "http://localhost:5200/hub/notifications";

const connection = new HubConnectionBuilder()
  .withUrl(HUB_URL)
  .withAutomaticReconnect() // Automatically tries to reconnect if the connection is lost.
  .configureLogging(LogLevel.Information)
  .build();

// Starts the connection to the SignalR hub.
export const startConnection = async () => {
  if (connection.state === 'Disconnected') {
    try {
      await connection.start();
      console.log("SignalR Connected successfully.");
    } catch (err) {
      console.error("SignalR Connection Error: ", err);
      // Optionally, retry connection after a delay.
      setTimeout(startConnection, 5000);
    }
  }
};

// Stops the connection.
export const stopConnection = () => {
  if (connection.state === 'Connected') {
    connection.stop();
  }
};

// Registers a handler that will be invoked when the hub sends a "NewItemAdded" message.
export const onNewItem = (callback) => {
  connection.on("NewItemAdded", callback);
};

// Registers a handler for "NewReviewAdded" messages.
export const onNewReview = (callback) => {
  connection.on("NewReviewAdded", callback);
};