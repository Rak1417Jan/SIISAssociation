import { USE_MOCK_DATA, RAZORPAY_KEY_ID } from '../config/apiConfig';

const loadRazorpayScript = () => {
  return new Promise((resolve) => {
    if (window.Razorpay) {
      resolve(true);
      return;
    }
    const script = document.createElement('script');
    script.src = 'https://checkout.razorpay.com/v1/checkout.js';
    script.onload = () => resolve(true);
    script.onerror = () => resolve(false);
    document.body.appendChild(script);
  });
};

export const openRazorpayModal = async (orderData, onSuccess, onFailure) => {
  if (USE_MOCK_DATA) {
    // Simulate 2 second delay for mock
    setTimeout(() => {
      onSuccess({
        razorpayOrderId: orderData.orderId,
        razorpayPaymentId: "pay_mock_" + Date.now(),
        razorpaySignature: "sig_mock"
      });
    }, 2000);
    return;
  }

  const isLoaded = await loadRazorpayScript();
  if (!isLoaded) {
    onFailure("Failed to load Razorpay SDK. Check your connection.");
    return;
  }

  // TODO: Replace test key with live key in production
  const options = {
    key: RAZORPAY_KEY_ID,
    amount: orderData.amount,
    currency: orderData.currency,
    order_id: orderData.orderId,
    prefill: orderData.prefill,
    handler: function (response) {
      onSuccess({
        razorpayOrderId: response.razorpay_order_id,
        razorpayPaymentId: response.razorpay_payment_id,
        razorpaySignature: response.razorpay_signature
      });
    },
    modal: {
      ondismiss: function () {
        onFailure("Payment cancelled");
      }
    }
  };

  const rzp = new window.Razorpay(options);
  rzp.on('payment.failed', function (response) {
    onFailure(response.error.description);
  });
  rzp.open();
};
