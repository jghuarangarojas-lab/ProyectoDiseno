import { useCallback, useEffect, useMemo, useState } from 'react';
import toast from 'react-hot-toast';
import { CartIcon, LeafIcon, SpinnerIcon } from '../components/icons';
import { InventoryTopBar } from '../components/inventory/InventoryTopBar';
import { ProductDetailsModal } from '../components/inventory/ProductDetailsModal';
import { ProductVisualCard } from '../components/inventory/ProductVisualCard';
import { RestockPanel } from '../components/inventory/RestockPanel';
import { useInventoryWebSockets } from '../hooks/useInventoryWebSockets';
import { useProductSearch } from '../hooks/useProductSearch';
import { useRestockList } from '../hooks/useRestockList';
import {
  inventoryService,
  toInventoryUserMessage,
} from '../services/inventory.service';
import type { RegisterEntryRequest, RegisterMermaRequest } from '../types/inventory';

type InventoryView = 'catalog' | 'restock';

export default function InventoryPage() {
  const [view, setView] = useState<InventoryView>('catalog');
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [isSubmittingEntry, setIsSubmittingEntry] = useState(false);
  const [isSubmittingMerma, setIsSubmittingMerma] = useState(false);

  const {
    products,
    suggestions,
    searchTerm,
    category,
    isLoading,
    isRefreshing,
    setSearchTerm,
    setCategory,
    refresh,
  } = useProductSearch();

  const {
    items: restockItems,
    isLoading: isRestockLoading,
    isRefreshing: isRestockRefreshing,
    lastLoadedAt: restockLoadedAt,
    refresh: refreshRestock,
  } = useRestockList();

  const { onLowStockAlert } = useInventoryWebSockets();

  useEffect(
    () =>
      onLowStockAlert(() => {
        void refresh(true);
        void refreshRestock(true);
      }),
    [onLowStockAlert, refresh, refreshRestock],
  );

  const selectedProduct = useMemo(
    () =>
      selectedProductId
        ? products.find((product) => product.id === selectedProductId) ?? null
        : null,
    [products, selectedProductId],
  );

  const handleSubmitEntry = useCallback(
    async (payload: RegisterEntryRequest): Promise<boolean> => {
      setIsSubmittingEntry(true);

      try {
        await inventoryService.registerEntryBatch(payload);

        const quantity = payload.items[0]?.quantity ?? 0;
        toast.success(`Lote de ${quantity} unidades registrado correctamente.`);

        await refresh(true);
        void refreshRestock(true);
        return true;
      } catch (error) {
        toast.error(toInventoryUserMessage(error));
        return false;
      } finally {
        setIsSubmittingEntry(false);
      }
    },
    [refresh, refreshRestock],
  );

  const handleSubmitMerma = useCallback(
    async (payload: RegisterMermaRequest): Promise<boolean> => {
      setIsSubmittingMerma(true);

      try {
        const merma = await inventoryService.registerMerma(payload);
        toast.success(
          `Merma registrada para ${merma.sku}: stock descontado y en revisión.`,
          { duration: 5000 },
        );

        await refresh(true);
        void refreshRestock(true);
        return true;
      } catch (error) {
        toast.error(toInventoryUserMessage(error));
        return false;
      } finally {
        setIsSubmittingMerma(false);
      }
    },
    [refresh, refreshRestock],
  );

  const handleOpenProduct = useCallback((productId: string) => {
    setSelectedProductId(productId);
  }, []);

  const handleCloseModal = useCallback(() => {
    setSelectedProductId(null);
  }, []);

  return (
    <div className="min-h-screen">
      <div className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl items-center gap-2 px-4 py-2.5">
          <button
            type="button"
            onClick={() => setView('catalog')}
            className={`flex items-center gap-2 rounded-full px-4 py-1.5 text-sm font-semibold transition ${
              view === 'catalog'
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            <LeafIcon className="h-4 w-4" />
            Catálogo Visual
          </button>

          <button
            type="button"
            onClick={() => setView('restock')}
            className={`flex items-center gap-2 rounded-full px-4 py-1.5 text-sm font-semibold transition ${
              view === 'restock'
                ? 'bg-rose-600 text-white shadow-sm'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            <CartIcon className="h-4 w-4" />
            Reposición
            {restockItems.length > 0 && (
              <span
                className={`rounded-full px-1.5 py-0.5 text-[10px] font-bold ${
                  view === 'restock'
                    ? 'bg-white text-rose-600'
                    : 'bg-rose-100 text-rose-600'
                }`}
              >
                {restockItems.length}
              </span>
            )}
          </button>
        </div>
      </div>

      {view === 'catalog' ? (
        <>
          <InventoryTopBar
            searchTerm={searchTerm}
            suggestions={suggestions}
            category={category}
            isRefreshing={isRefreshing}
            onSearchTermChange={setSearchTerm}
            onSelectSuggestion={(product) => handleOpenProduct(product.id)}
            onCategoryChange={setCategory}
            onRefresh={() => void refresh()}
          />

          {isLoading ? (
            <div className="flex items-center justify-center gap-2 py-24 text-slate-500">
              <SpinnerIcon className="h-5 w-5 animate-spin" />
              <span className="text-sm">Cargando catálogo...</span>
            </div>
          ) : products.length === 0 ? (
            <div className="mx-auto flex max-w-md flex-col items-center gap-2 px-4 py-24 text-center">
              <div className="flex h-14 w-14 items-center justify-center rounded-full bg-slate-100">
                <LeafIcon className="h-7 w-7 text-slate-400" />
              </div>
              <p className="font-semibold text-slate-700">
                Sin resultados para la búsqueda
              </p>
              <p className="text-sm text-slate-400">
                Ajusta el nombre o quita el filtro de categoría para ver más
                productos.
              </p>
            </div>
          ) : (
            <div className="mx-auto max-w-7xl px-4 py-6">
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
                {products.map((product) => (
                  <ProductVisualCard
                    key={product.id}
                    product={product}
                    onSelect={(item) => handleOpenProduct(item.id)}
                  />
                ))}
              </div>
            </div>
          )}
        </>
      ) : (
        <RestockPanel
          items={restockItems}
          isLoading={isRestockLoading}
          isRefreshing={isRestockRefreshing}
          lastLoadedAt={restockLoadedAt}
          onRefresh={() => void refreshRestock()}
        />
      )}

      {selectedProduct && (
        <ProductDetailsModal
          product={selectedProduct}
          isSubmittingEntry={isSubmittingEntry}
          isSubmittingMerma={isSubmittingMerma}
          onClose={handleCloseModal}
          onSubmitEntry={handleSubmitEntry}
          onSubmitMerma={handleSubmitMerma}
        />
      )}
    </div>
  );
}
