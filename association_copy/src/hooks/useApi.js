import { useState, useEffect, useCallback } from 'react';

/**
 * Wraps a service call and manages loading/error/data states.
 * @param {Function} serviceFn - The service function to call
 * @param {any} [params] - When provided, auto-fetches on mount and when params change
 * @returns {{ data: any, loading: boolean, error: any, refetch: Function, execute: Function }}
 */
export function useApi(serviceFn, params) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const shouldAutoFetch = arguments.length > 1;

  const execute = useCallback(
    async (...callParams) => {
      setLoading(true);
      setError(null);

      try {
        const resolvedParams =
          callParams.length > 0
            ? callParams
            : Array.isArray(params)
              ? params
              : params !== undefined
                ? [params]
                : [];

        const result = await serviceFn(...resolvedParams);

        if (result.error) {
          setError(result.error);
        } else {
          setData(result.data);
        }
        return result;
      } catch (err) {
        const message = err.message || 'Unknown error';
        setError(message);
        return { data: null, error: message, status: 500 };
      } finally {
        setLoading(false);
      }
    },
    [serviceFn, JSON.stringify(params)]
  );

  useEffect(() => {
    if (shouldAutoFetch) {
      execute();
    }
  }, [execute, shouldAutoFetch]);

  return { data, loading, error, refetch: execute, execute };
}
