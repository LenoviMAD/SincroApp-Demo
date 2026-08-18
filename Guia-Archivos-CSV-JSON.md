# Guía de Carga de Archivos CSV y JSON — SincroDatosApp v4.0

Documentación del formato de cada archivo que SincroDatosApp lee y envía al IntegradorArchivosApi.

---

## Configuración inicial — appconfig.json

Antes de usar la aplicación, configurar el archivo `appconfig.json` en la carpeta de instalación:

| Campo | Descripción |
|---|---|
| `ApiUrl` | URL base del IntegradorArchivosApi (ej: `http://localhost:5080`) |
| `ApiUsuario` | Usuario asignado a esta empresa (se gestiona desde el portal WebMultiempresa) |
| `ApiClave` | Clave del usuario. El sistema la hashea internamente — nunca se guarda en texto plano |
| `PathArchivoTx` | Carpeta donde se depositan los archivos CSV/JSON a sincronizar |
| `DownloadFolder` | Carpeta donde se guardan los pedidos descargados |
| `IntervaloSubidaSegundos` | Frecuencia de subida automática (segundos) |
| `IntervaloDescargaSegundos` | Frecuencia de descarga de pedidos (segundos) |

Al iniciar, SincroDatosApp hace login automático contra el servidor. Si el usuario/clave son incorrectos, la aplicación no arranca y muestra un mensaje de error. **El EmpresaID nunca se configura manualmente** — el servidor lo resuelve a partir de las credenciales.

---

## Archivos soportados

SincroDatosApp detecta el tipo de archivo por el nombre:

| Nombre contiene | Tipo | Endpoint API |
|---|---|---|
| `productos` | CSV Productos | `POST /api/sync/productos` |
| `clientes` | CSV Clientes | `POST /api/sync/clientes` |
| `stock` | CSV Stock | `POST /api/sync/stock` |
| `maestros` | JSON Maestros | `POST /api/sync/maestros` |

**Vendedores**: NO son procesados por SincroDatosApp. Se cargan desde el portal WebMultiempresa.

---

## CSV Productos

**Nombre de archivo**: debe contener `productos` (ej: `productos_20260101.csv`)

**Separador**: `;`
**Encoding**: UTF-8 con BOM
**Primera fila**: encabezado (ignorada al parsear)

### Columnas

| # | Campo | Tipo | Descripción |
|---|---|---|---|
| 0 | `ProductosID` | int | ID del producto en el ERP origen |
| 1 | `ProductosIDPadre` | int | ID del producto padre (0 si no tiene) |
| 2 | `CodigoDeProducto` | string | Código único del producto (clave natural de MERGE) |
| 3 | `Nombre` | string | Nombre del producto |
| 4 | `StockEnUnidades` | int | Stock en unidades al momento de exportar |
| 5 | `CantidadMultiplo` | int | Múltiplo de venta |
| 6 | `DesactivadoParaLaVenta` | bool | `true`/`false` |
| 7 | `MarcasProductosID` | int | FK a tabla MarcasProductos |
| 8 | `FamiliaProductosID` | int | FK a tabla FamiliaProductos |
| 9 | `StockPropio` | bool | `true` si el stock es gestionado por esta empresa |
| 10 | `CategoriaComercialID` | int | FK a tabla CategoriasComerciales |
| 11 | `UnidadesPorBulto` | int | Unidades por bulto / caja |
| 12 | `PrecioProducto` | decimal | Precio costo. El API calcula precios por lista aplicando `PorcentajeMarcup` |
| 13 | `ImpuestosID` | int | FK a tabla Impuestos |
| 14 | `Baja` | bool | `true` si el producto está dado de baja |
| 15 | `CategoriasProductosID` | int | FK a tabla CategoriaProductos |
| 16 | `SubCategoriasProductosID` | int | FK a tabla SubCategoriaProductos |

### Qué hace el API al recibir productos
1. MERGE en `Productos` por `(EmpresaID, Codigo)`
2. Calcula y hace MERGE en `ProductoPrecios` por cada lista de precios activa
3. MERGE en `ProductoSubCategorias` (col 16)
4. MERGE en `ProductoCategoriasComerciales` (col 10 → `CategoriasComercialesID`)

---

## CSV Clientes

**Nombre de archivo**: debe contener `clientes` (ej: `clientes_20260101.csv`)

**Separador**: `;`
**Encoding**: UTF-8 con BOM
**Primera fila**: encabezado

### Columnas

| # | Campo | Tipo | Descripción |
|---|---|---|---|
| 0 | `ClientesID` | int | ID del cliente en el ERP origen |
| 1 | `Codigo` | string | Código del cliente (clave natural de MERGE) |
| 2 | `Nombre` | string | Nombre o razón social |
| 3 | `Direccion` | string | Dirección comercial |
| 4 | `Localidad` | string | Localidad |
| 5 | `Telefono` | string | Teléfono de contacto |
| 6 | `DiaDeVenta` | int | Día de la semana de visita (0=Dom … 6=Sáb) |
| 7 | `PorcentajePercepcionIB` | decimal | % de percepción ingresos brutos |
| 8 | `Latitud` | double | Coordenada GPS (InvariantCulture) |
| 9 | `Longitud` | double | Coordenada GPS (InvariantCulture) |
| 10 | `ListasPreciosID` | int | ID de la lista de precios principal |
| 11 | `TiposDocumentosID` | int | FK a TiposDocumentos (1=CUIT, 2=DNI, etc.) |
| 12 | `NumeroDocumento` | string | Número de CUIT/DNI |
| 13 | `Baja` | bool | `true` si el cliente está dado de baja |

### Qué hace el API al recibir clientes
1. MERGE en `Clientes` por `(EmpresaID, Codigo)`
2. MERGE en `ClienteDocumentos` (cols 11 y 12, si ambos presentes)
3. MERGE en `ClienteListasPrecios` marcando la lista como `EsPrincipal=1`

---

## CSV Stock

**Nombre de archivo**: debe contener `stock` (ej: `stock_20260101.csv`)

**Separador**: `;`
**Encoding**: UTF-8 con BOM
**Primera fila**: encabezado

### Columnas

| # | Campo | Tipo | Descripción |
|---|---|---|---|
| 0 | `ProductosID` | int | ID del producto (usado si CodigoProducto está vacío) |
| 1 | `CodigoProducto` | string | Código del producto (preferido sobre ProductosID) |
| 2 | `StockUnidades` | decimal | Cantidad en stock. Reemplaza el valor anterior (no acumula) |

> **Solo se usa 1 campo de stock** (`StockUnidades`). El API actualiza `StockEnUnidades` en la tabla `Productos`.
> Si `CodigoProducto` no está vacío, el UPDATE filtra por `(EmpresaID, Codigo)`. Si está vacío, filtra por `(EmpresaID, ProductosID)`.
> Lógica especial: si el stock anterior era 0 y el nuevo es > 0, actualiza `FechaUltimoReingreso` automáticamente.

---

## JSON Maestros

**Nombre de archivo**: debe contener `maestros` (ej: `maestros.json`)

**Formato**: JSON
**Endpoint**: `POST /api/sync/maestros`

Carga todos los catálogos de referencia de una vez. Ejecutar **antes** de cargar productos y clientes.

> El campo `empresaID` **no va en el JSON** — la aplicación lo inyecta automáticamente en base a las credenciales de login.

### Estructura del JSON

```json
{
  "impuestos": [
    { "impuestosID": 1, "nombre": "IVA 21%", "porcentaje": 21.00 }
  ],
  "marcasProductos": [
    { "marcasProductosID": 1, "nombre": "Sin Marca" }
  ],
  "familiasProductos": [
    { "familiaProductosID": 1, "nombre": "Aceites" }
  ],
  "categorias": [
    { "categoriasProductosID": 1, "nombre": "Almacen" }
  ],
  "subCategorias": [
    { "subCategoriasProductosID": 1, "nombre": "Aceites y Vinagres" }
  ],
  "categoriasComerciales": [
    { "categoriasComercialesID": 1, "nombre": "Categoria Comercial 1" }
  ],
  "listasPrecios": [
    {
      "ListasPreciosID": 1,
      "Nombre": "Lista Minorista",
      "Baja": false,
      "MontoMinimo": 5000.00,
      "PorcentajeMarcup": 21.00
    }
  ],
  "tiposDocumentos": [
    { "tiposDocumentosID": 1, "nombre": "CUIT" }
  ]
}
```

### Descripción de secciones

| Sección JSON | Tabla en BD | Notas |
|---|---|---|
| `impuestos` | `Impuestos` | MERGE por `(EmpresaID, ImpuestosID)` |
| `marcasProductos` | `MarcasProductos` | MERGE por `(EmpresaID, MarcasProductosID)` |
| `familiasProductos` | `FamiliaProductos` | MERGE por `(EmpresaID, FamiliaProductosID)` |
| `categorias` | `CategoriaProductos` | MERGE por `(EmpresaID, CategoriasProductosID)` |
| `subCategorias` | `SubCategoriaProductos` | MERGE por `(EmpresaID, SubCategoriasProductosID)` |
| `categoriasComerciales` | `CategoriasComerciales` | MERGE por `(EmpresaID, CategoriasComercialesID)` |
| `listasPrecios` | `ListasPrecios` | MERGE por `(EmpresaID, ListasPreciosID)`. `PorcentajeMarcup` se usa para calcular precios de productos |
| `tiposDocumentos` | `TiposDocumentos` | Catálogo global sin EmpresaID |

---

## Orden de carga recomendado

1. `maestros.json` — catálogos de referencia
2. `productos_*.csv` — productos con precios
3. `stock_*.csv` — stock actualizado
4. `clientes_*.csv` — clientes con listas de precios

---

## Notas sobre Vendedores

Los vendedores **no se cargan por SincroDatosApp**. Se populan mediante:
- El portal WebMultiempresa (alta/modificación individual)
- Seeds SQL en `sql-seed/Seed_Vendedores.sql` para entornos nuevos

Las relaciones asociadas (`ClienteVendedores`, `VendedorListasPrecios`, `VendedorEstadisticas`, `VendedorEstrellas`) también son gestionadas por el portal.
