export interface DeliveryOrderItem {
  foodItemId: number;
  foodItemName: string;
  price: number;
  quantity: number;
  imageUrl: string;
}

export interface DeliveryOrder {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: string; // Assigned, Accepted, ReachedRestaurant, PickedUp, OutForDelivery, Delivered, Rejected
  totalAmount: number;
  deliveryCharge: number;
  finalAmount: number;
  paymentMethod: string;
  paymentStatus: string;
  userId: number;
  customerName: string;
  customerPhone: string;
  deliveryAddress: string;
  restaurantId: number;
  restaurantName: string;
  restaurantAddress: string;
  restaurantPhone: string;
  restaurantImageUrl: string;
  deliveryPartnerId?: number;
  acceptedAt?: string;
  reachedRestaurantAt?: string;
  pickedUpAt?: string;
  outForDeliveryAt?: string;
  deliveredAt?: string;
  customerRating?: number;
  customerFeedback?: string;
  items: DeliveryOrderItem[];
}

export interface DailyEarningBreakdown {
  date: string;
  amount: number;
  deliveriesCount: number;
}

export interface RecentPayout {
  id: number;
  date: string;
  amount: number;
  status: string;
  referenceNumber: string;
}

export interface DeliveryEarnings {
  todayEarnings: number;
  weeklyEarnings: number;
  monthlyEarnings: number;
  totalEarnings: number;
  todayDeliveriesCount: number;
  totalDeliveriesCount: number;
  averageRating: number;
  dailyBreakdown: DailyEarningBreakdown[];
  recentPayouts: RecentPayout[];
}

export interface DeliveryProfile {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  profileImageUrl: string;
  isOnline: boolean;
  vehicleType: string;
  vehicleNumber: string;
  vehicleModel: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  bankAccountHolder: string;
  bankName: string;
  accountNumber: string;
  ifscCode: string;
  upiId: string;
}

export interface UpdateDeliveryProfileRequest {
  fullName: string;
  phoneNumber: string;
  profileImageUrl?: string;
  vehicleType: string;
  vehicleNumber: string;
  vehicleModel: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  bankAccountHolder: string;
  bankName: string;
  accountNumber: string;
  ifscCode: string;
  upiId: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
