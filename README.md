# InventarioAPI

## Filtro de productos por bodega

El endpoint `GET /api/productos` permite filtrar por la bodega asignada usando query string.

Ejemplos:

- `GET /api/productos?bodega=Central`
- `GET /api/productos?search=teclado&categoria=Perifericos&bodega=Sur&soloBajoStock=true`
- `GET /api/productos/resumen?bodega=Norte`

## Catalogos compartidos

- `GET /api/categorias`
- `GET /api/bodegas`

## Indicadores de rotacion

- `GET /api/productos/rotacion`
- `GET /api/productos/rotacion?desde=2026-01-01&hasta=2026-12-31&top=10`