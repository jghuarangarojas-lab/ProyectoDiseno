import React, { useState } from 'react';
import { ShoppingCartPanel } from '../components/cart/ShoppingCartPanel';
import { SearchInput } from '../components/common/SearchInput';
import { PosProductCard } from '../components/pos/PosProductCard';
import { ManualPriceModal } from '../components/cart/ManualPriceModal';
import { CheckoutModal, CheckoutData } from '../components/pos/CheckoutModal';
import { SuccessScreen } from '../components/pos/SuccessScreen';
import { useCart } from '../contexts/CartContext';
import { CartItem, Product, SaleResponse } from '../types/pos';
import { useQrScanner } from '../hooks/useQrScanner';
import { posService } from '../services/posService';

const MOCK_PRODUCTS: Product[] = [
  {
    id: '1',
    sku: 'PLT-001',
    name: 'Planta de Helecho',
    priceMinorista: 15.00,
    priceMayorista: 12.00,
    imageUrl: 'https://via.placeholder.com/300x200?text=Helecho',
    stock: 50,
  },
  {
    id: '2',
    sku: 'MAC-001',
    name: 'Maceta Cerámica 20cm',
    priceMinorista: 45.00,
    priceMayorista: 38.00,
    imageUrl: 'https://via.placeholder.com/300x200?text=Maceta',
    stock: 30,
  },
  {
    id: '3',
    sku: 'ABO-001',
    name: 'Abono Orgánico 5kg',
    priceMinorista: 25.00,
    priceMayorista: 20.00,
    imageUrl: 'https://via.placeholder.com/300x200?text=Abono',
    stock: 100,
  },
  {
    id: '4',
    sku: 'TIE-001',
    name: 'Tierra de Jardín',
    priceMinorista: 18.00,
    priceMayorista: 14.00,
    imageUrl: 'https://via.placeholder.com/300x200?text=Tierra',
    stock: 80,
  },
];

export const PosPage: React.FC = () => {
  const { items, addToCart, updateManualPrice, clearCart, total } = useCart();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedItem, setSelectedItem] = useState<CartItem | null>(null);
  const [isManualModalOpen, setIsManualModalOpen] = useState(false);
  const [isCheckoutModalOpen, setIsCheckoutModalOpen] = useState(false);
  const [completedSale, setCompletedSale] = useState<SaleResponse | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);

  useQrScanner((code) => {
    console.log('QR Scanned:', code);
    const product = MOCK_PRODUCTS.find(p => p.sku === code || p.id === code);
    if (product) {
      addToCart(product);
    }
  });

  const filteredProducts = MOCK_PRODUCTS.filter(product =>
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.sku.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleManualAdjust = (item: CartItem) => {
    setSelectedItem(item);
    setIsManualModalOpen(true);
  };

  const handleManualPriceConfirm = (itemId: string, newPrice: number, reason: string) => {
    updateManualPrice(itemId, newPrice, reason);
    setIsManualModalOpen(false);
  };

  const handleCheckout = () => {
    setIsCheckoutModalOpen(true);
  };

  const handleCheckoutConfirm = async (data: CheckoutData) => {
    setIsProcessing(true);
    try {
      const saleRequest = {
        totalAmount: total,
        paymentMethod: data.paymentMethod,
        documentType: data.documentType,
        documentCorrelative: `${data.documentType === 'Boleta' ? 'B001' : 'F001'}-00001`,
        isDelivery: data.isDelivery,
        saleDetails: items.map(item => ({
          productSku: item.product.sku,
          quantity: item.quantity,
          originalPrice: item.originalPrice,
          finalPrice: item.finalPrice,
          isManualOverride: item.isManualOverride,
          overrideReason: item.overrideReason,
        })),
        deliveryAddress: data.deliveryAddress,
        deliveryDate: data.deliveryDate,
        deliveryTime: data.deliveryTime,
        deliveryPhone: data.deliveryPhone,
        deliveryReference: data.deliveryReference,
      };

      const sale = await posService.processSale(saleRequest);
      setCompletedSale(sale);
      setIsCheckoutModalOpen(false);
    } catch (error) {
      console.error('Error al procesar venta:', error);
      alert('Error al procesar la venta. Intente nuevamente.');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleNewSale = () => {
    clearCart();
    setCompletedSale(null);
    setSearchTerm('');
  };

  const handlePrint = () => {
    if (completedSale?.pdfUrl) {
      window.open(completedSale.pdfUrl, '_blank');
    } else {
      alert('PDF no disponible aún');
    }
  };

  if (completedSale) {
    return (
      <SuccessScreen
        sale={completedSale}
        onNewSale={handleNewSale}
        onPrint={handlePrint}
      />
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="flex h-screen">
        <div className="w-[70%] p-4 overflow-y-auto">
          <div className="mb-4">
            <SearchInput value={searchTerm} onChange={setSearchTerm} />
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {filteredProducts.map(product => (
              <PosProductCard
                key={product.id}
                product={product}
                onAddToCart={addToCart}
              />
            ))}
          </div>
          {filteredProducts.length === 0 && (
            <p className="text-center text-gray-500 mt-8">No se encontraron productos</p>
          )}
        </div>
        <div className="w-[30%] p-4 bg-white border-l border-gray-200">
          <ShoppingCartPanel
            onManualAdjust={handleManualAdjust}
            onCheckout={handleCheckout}
          />
        </div>
      </div>
      <ManualPriceModal
        isOpen={isManualModalOpen}
        item={selectedItem}
        onClose={() => setIsManualModalOpen(false)}
        onConfirm={handleManualPriceConfirm}
      />
      <CheckoutModal
        isOpen={isCheckoutModalOpen}
        onClose={() => setIsCheckoutModalOpen(false)}
        onConfirm={handleCheckoutConfirm}
        isProcessing={isProcessing}
      />
    </div>
  );
};
