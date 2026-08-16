export interface User {
  id?: number;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role?: string;
  profileImageUrl?: string;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role?: string;
  userId?: number;
  profileImageUrl?: string;
}

export interface Restaurant {
  id: number;
  name: string;
  description: string;
  address: string;
  city: string;
  state?: string;
  pincode?: string;
  ownerName?: string;
  mobileNumber?: string;
  email?: string;
  phoneNumber?: string;
  rating?: number;
  imageUrl?: string;
  isOpen: boolean;
  cuisineType?: string;
  deliveryTime?: number;
  deliveryCharge?: number;
  discountAmount: number;
  minimumOrderAmount?: number;
  averageCostForTwo?: number;
  openingTime?: string;
  closingTime?: string;
  hasOrders?: boolean;
  hasFoodItems?: boolean;
}

export interface CreateRestaurantRequest {
  name: string;
  description: string;
  address: string;
  city: string;
  state?: string;
  pincode?: string;
  ownerName?: string;
  mobileNumber?: string;
  email?: string;
  phoneNumber?: string;
  cuisineType?: string;
  isOpen?: boolean;
  deliveryTime?: number;
  deliveryCharge?: number;
  minimumOrderAmount?: number;
  averageCostForTwo?: number;
  openingTime?: string;
  closingTime?: string;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  imageUrl?: string;
  restaurantId?: number;
  displayOrder?: number;
  isActive?: boolean;
  hasFoodItems?: boolean;
}

export interface FoodItem {
  id: number;
  name: string;
  description: string;
  price: number;
  offerPrice?: number;
  imageUrl?: string;
  image?: string;
  isVegetarian: boolean;
  isAvailable: boolean;
  categoryId: number;
  categoryName?: string;
  restaurantId: number;
  restaurantName?: string;
  hasOrders?: boolean;
}

export interface CreateFoodItemRequest {
  name: string;
  description: string;
  price: number;
  offerPrice?: number;
  isVegetarian: boolean;
  isAvailable: boolean;
  categoryId: number;
  restaurantId: number;
}

export interface CartItem {
  id: number;
  foodItemId: number;
  foodItemName: string;
  foodItemPrice: number;
  quantity: number;
  imageUrl?: string;
  restaurantId?: number;
  isVegetarian?: boolean;
}

export interface Cart {
  cartId?: number;
  items: CartItem[];
  totalPrice: number;
}

export interface Address {
  id: number;
  fullName?: string;
  phoneNumber?: string;
  houseNo?: string;
  street?: string;
  landmark?: string;
  city: string;
  state?: string;
  pincode?: string;
  postalCode?: string;
  addressLine?: string;
  addressType?: string;
  isDefault?: boolean;
}

export interface Coupon {
  id: number;
  code: string;
  discountPercentage: number;
  minOrderAmount: number;
  maxDiscountAmount: number;
  expiryDate?: string;
  isActive?: boolean;
}

export interface OrderItem {
  id: number;
  foodItemId: number;
  foodItemName: string;
  foodItemPrice: number;
  quantity: number;
}

export interface Order {
  id: number;
  orderNumber?: string;
  userId?: number;
  userName?: string;
  userEmail?: string;
  userPhone?: string;
  phoneNumber?: string;
  orderDate: string;
  totalAmount: number;
  deliveryCharge: number;
  discount: number;
  tax: number;
  finalAmount: number;
  status: string;
  deliveryAddress: string;
  restaurantName?: string;
  items: OrderItem[];
}

export interface PlaceOrderRequest {
  deliveryAddress: string;
  phoneNumber: string;
  paymentMethod: number;
}

export interface Review {
  id: number;
  userId?: number;
  userName: string;
  userImageUrl?: string;
  foodItemId?: number;
  foodName?: string;
  foodImageUrl?: string;
  restaurantName?: string;
  restaurantImageUrl?: string;
  rating: number;
  comment: string;
  createdAt?: string;
  createdOn?: string;
}

export interface AddReviewRequest {
  foodItemId: number;
  rating: number; 
  comment: string;
}

export interface UserMaster {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  roleId: number;
  roleName: string;
  isActive: boolean;
  emailVerified: boolean;
  lastLogin?: string;
  imageUrl?: string;
  createdOn: string;
  ordersCount: number;
  addressesCount: number;
  reviewsCount: number;
}

export interface UserStats {
  totalUsers: number;
  totalSuperAdmins: number;
  totalAdmins: number;
  totalCustomers: number;
  totalDeliveryPartners: number;
  totalRestaurantOwners: number;
  activeUsers: number;
  inactiveUsers: number;
}

export interface RoleMaster {
  id: number;
  name: string;
  description?: string;
  usersCount: number;
  isActive?: boolean;
}

export interface DeliveryPartnerMasterItem {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  profileImageUrl?: string;
  isOnline: boolean;
  vehicleType: string;
  vehicleNumber: string;
  vehicleModel: string;
  licenseNumber: string;
  licenseExpiryDate?: string;
  bankAccountHolder?: string;
  bankName?: string;
  accountNumber?: string;
  ifscCode?: string;
  upiId?: string;
  rating?: number;
  totalDeliveries?: number;
}

export * from './permission.model';
