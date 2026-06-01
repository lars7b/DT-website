// OrdersPage.jsx
import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Navbar from '../components/Navbar';

// this page is for viewing orders
export default function OrdersPage(){
    // State variabelen voor het opslaan van de database gegevens en laad-statussen
    const [orders, setOrders] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {}, []);

}
