import React, { useEffect } from 'react';
import { 
  startConnection, 
  stopConnection, 
  onNewItem, 
  onNewReview,
} from '../services/notificationService';

/**
 * A component to manage the SignalR connection and handle incoming real-time events.
 * It doesn't render any UI itself.
 */
const RealTimeNotifications = () => {

  useEffect(() => {
    // Start the connection when the component mounts.
    startConnection();

    const handleNewItem = (item) => {
      console.log('Real-time event: A new item was added!', item);
      // Here, you could show a toast notification or update a global state.
    };

    const handleNewReview = (review) => {
      console.log('Real-time event: A new review was submitted!', review);
      // e.g., show a notification or update the relevant item's review list.
    };

    // Register the event listeners.
    onNewItem(handleNewItem);
    onNewReview(handleNewReview);

    // The cleanup function will be called when the component unmounts.
    return () => {
      stopConnection();
    };
  }, []); // The empty dependency array ensures this effect runs only once.

  return null; // This component is for logic only, no UI.
};

export default RealTimeNotifications;