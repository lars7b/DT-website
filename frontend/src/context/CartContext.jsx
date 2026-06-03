// // src/context/CartContext.jsx

// const CartContext = createContext();

// export function CartProvider({ children }) {
//   const [cart, setCart] = useState(null);
//   const [totalItems, setTotalItems] = useState(0);

//   const refreshCart = async () => {
//     const token = localStorage.getItem("token");

//     const response = await fetch(
//       `${import.meta.env.VITE_API_URL}/api/shoppingcart`,
//       {
//         headers: {
//           Authorization: `Bearer ${token}`,
//         },
//       },
//     );

//     if (!response.ok) return;

//     const data = await response.json();

//     setCart(data);

//     setTotalItems(data.items.reduce((sum, item) => sum + item.quantity, 0));
//   };
//   return (
//     <CartContext.Provider value={{ cart, totalItems, refreshCart }}>
//       {children}
//     </CartContext.Provider>
//   );
// }

// export function useCart() {
//     return useContext(CartContext);
// }