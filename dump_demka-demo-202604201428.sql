--
-- PostgreSQL database dump
--

-- Dumped from database version 15.6
-- Dumped by pg_dump version 15.6

-- Started on 2026-04-20 14:28:19

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 9 (class 2615 OID 30583)
-- Name: public3; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA public3;


ALTER SCHEMA public3 OWNER TO postgres;

--
-- TOC entry 332 (class 1255 OID 30585)
-- Name: update_updated_at_column(); Type: FUNCTION; Schema: public3; Owner: postgres
--

CREATE FUNCTION public3.update_updated_at_column() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION public3.update_updated_at_column() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 302 (class 1259 OID 30709)
-- Name: cart_items; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.cart_items (
    id integer NOT NULL,
    cart_id integer NOT NULL,
    product_id integer NOT NULL,
    quantity integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT cart_items_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public3.cart_items OWNER TO postgres;

--
-- TOC entry 301 (class 1259 OID 30708)
-- Name: cart_items_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.cart_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.cart_items_id_seq OWNER TO postgres;

--
-- TOC entry 3655 (class 0 OID 0)
-- Dependencies: 301
-- Name: cart_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.cart_items_id_seq OWNED BY public3.cart_items.id;


--
-- TOC entry 300 (class 1259 OID 30693)
-- Name: carts; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.carts (
    id integer NOT NULL,
    customer_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public3.carts OWNER TO postgres;

--
-- TOC entry 299 (class 1259 OID 30692)
-- Name: carts_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.carts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.carts_id_seq OWNER TO postgres;

--
-- TOC entry 3656 (class 0 OID 0)
-- Dependencies: 299
-- Name: carts_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.carts_id_seq OWNED BY public3.carts.id;


--
-- TOC entry 288 (class 1259 OID 30587)
-- Name: categories; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.categories (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public3.categories OWNER TO postgres;

--
-- TOC entry 287 (class 1259 OID 30586)
-- Name: categories_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.categories_id_seq OWNER TO postgres;

--
-- TOC entry 3657 (class 0 OID 0)
-- Dependencies: 287
-- Name: categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.categories_id_seq OWNED BY public3.categories.id;


--
-- TOC entry 296 (class 1259 OID 30650)
-- Name: customers; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.customers (
    id integer NOT NULL,
    fullname text NOT NULL,
    email character varying(100) NOT NULL,
    phone text NOT NULL,
    roleid integer,
    address text,
    password text,
    city text,
    image_path text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public3.customers OWNER TO postgres;

--
-- TOC entry 295 (class 1259 OID 30649)
-- Name: customers_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.customers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.customers_id_seq OWNER TO postgres;

--
-- TOC entry 3658 (class 0 OID 0)
-- Dependencies: 295
-- Name: customers_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.customers_id_seq OWNED BY public3.customers.id;


--
-- TOC entry 298 (class 1259 OID 30671)
-- Name: favorites; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.favorites (
    id integer NOT NULL,
    customer_id integer NOT NULL,
    product_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public3.favorites OWNER TO postgres;

--
-- TOC entry 297 (class 1259 OID 30670)
-- Name: favorites_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.favorites_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.favorites_id_seq OWNER TO postgres;

--
-- TOC entry 3659 (class 0 OID 0)
-- Dependencies: 297
-- Name: favorites_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.favorites_id_seq OWNED BY public3.favorites.id;


--
-- TOC entry 310 (class 1259 OID 30809)
-- Name: notifications; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.notifications (
    id integer NOT NULL,
    customer_id integer NOT NULL,
    title character varying(200) NOT NULL,
    message text NOT NULL,
    is_read boolean DEFAULT false NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    read_at timestamp without time zone
);


ALTER TABLE public3.notifications OWNER TO postgres;

--
-- TOC entry 309 (class 1259 OID 30808)
-- Name: notifications_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.notifications_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.notifications_id_seq OWNER TO postgres;

--
-- TOC entry 3660 (class 0 OID 0)
-- Dependencies: 309
-- Name: notifications_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.notifications_id_seq OWNED BY public3.notifications.id;


--
-- TOC entry 306 (class 1259 OID 30757)
-- Name: order_items; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.order_items (
    id integer NOT NULL,
    order_id integer NOT NULL,
    product_id integer NOT NULL,
    quantity integer NOT NULL,
    price_at_time numeric(10,2) NOT NULL,
    discount_applied integer DEFAULT 0 NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT order_items_price_at_time_check CHECK ((price_at_time >= (0)::numeric)),
    CONSTRAINT order_items_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public3.order_items OWNER TO postgres;

--
-- TOC entry 305 (class 1259 OID 30756)
-- Name: order_items_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.order_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.order_items_id_seq OWNER TO postgres;

--
-- TOC entry 3661 (class 0 OID 0)
-- Dependencies: 305
-- Name: order_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.order_items_id_seq OWNED BY public3.order_items.id;


--
-- TOC entry 292 (class 1259 OID 30610)
-- Name: order_statuses; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.order_statuses (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    sort_order integer DEFAULT 0,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public3.order_statuses OWNER TO postgres;

--
-- TOC entry 291 (class 1259 OID 30609)
-- Name: order_statuses_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.order_statuses_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.order_statuses_id_seq OWNER TO postgres;

--
-- TOC entry 3662 (class 0 OID 0)
-- Dependencies: 291
-- Name: order_statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.order_statuses_id_seq OWNED BY public3.order_statuses.id;


--
-- TOC entry 304 (class 1259 OID 30730)
-- Name: orders; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.orders (
    id integer NOT NULL,
    customer_id integer NOT NULL,
    order_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    status_id integer NOT NULL,
    total_amount numeric(10,2) NOT NULL,
    shipping_address text,
    city character varying(50),
    notes text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT orders_total_amount_check CHECK ((total_amount >= (0)::numeric))
);


ALTER TABLE public3.orders OWNER TO postgres;

--
-- TOC entry 303 (class 1259 OID 30729)
-- Name: orders_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.orders_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.orders_id_seq OWNER TO postgres;

--
-- TOC entry 3663 (class 0 OID 0)
-- Dependencies: 303
-- Name: orders_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.orders_id_seq OWNED BY public3.orders.id;


--
-- TOC entry 294 (class 1259 OID 30623)
-- Name: products; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.products (
    id integer NOT NULL,
    category_id integer NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    price numeric(10,2) NOT NULL,
    discount_percent integer DEFAULT 0 NOT NULL,
    discount_start_date timestamp without time zone,
    discount_end_date timestamp without time zone,
    stock_quantity integer DEFAULT 0 NOT NULL,
    image_url text,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT products_discount_percent_check CHECK (((discount_percent >= 0) AND (discount_percent <= 100))),
    CONSTRAINT products_price_check CHECK ((price >= (0)::numeric)),
    CONSTRAINT products_stock_quantity_check CHECK ((stock_quantity >= 0))
);


ALTER TABLE public3.products OWNER TO postgres;

--
-- TOC entry 293 (class 1259 OID 30622)
-- Name: products_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.products_id_seq OWNER TO postgres;

--
-- TOC entry 3664 (class 0 OID 0)
-- Dependencies: 293
-- Name: products_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.products_id_seq OWNED BY public3.products.id;


--
-- TOC entry 308 (class 1259 OID 30780)
-- Name: reviews; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.reviews (
    id integer NOT NULL,
    product_id integer NOT NULL,
    customer_id integer NOT NULL,
    rating integer NOT NULL,
    comment text,
    is_verified_purchase boolean DEFAULT false,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT reviews_rating_check CHECK (((rating >= 1) AND (rating <= 5)))
);


ALTER TABLE public3.reviews OWNER TO postgres;

--
-- TOC entry 307 (class 1259 OID 30779)
-- Name: reviews_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.reviews_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.reviews_id_seq OWNER TO postgres;

--
-- TOC entry 3665 (class 0 OID 0)
-- Dependencies: 307
-- Name: reviews_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.reviews_id_seq OWNED BY public3.reviews.id;


--
-- TOC entry 290 (class 1259 OID 30601)
-- Name: roles; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.roles (
    roleid integer NOT NULL,
    rolename text
);


ALTER TABLE public3.roles OWNER TO postgres;

--
-- TOC entry 289 (class 1259 OID 30600)
-- Name: roles_roleid_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.roles_roleid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.roles_roleid_seq OWNER TO postgres;

--
-- TOC entry 3666 (class 0 OID 0)
-- Dependencies: 289
-- Name: roles_roleid_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.roles_roleid_seq OWNED BY public3.roles.roleid;


--
-- TOC entry 312 (class 1259 OID 30828)
-- Name: suppliers; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.suppliers (
    id integer NOT NULL,
    name character varying(150) NOT NULL,
    contact_person character varying(100),
    phone text,
    email character varying(100),
    address text,
    city character varying(50),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public3.suppliers OWNER TO postgres;

--
-- TOC entry 311 (class 1259 OID 30827)
-- Name: suppliers_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.suppliers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.suppliers_id_seq OWNER TO postgres;

--
-- TOC entry 3667 (class 0 OID 0)
-- Dependencies: 311
-- Name: suppliers_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.suppliers_id_seq OWNED BY public3.suppliers.id;


--
-- TOC entry 314 (class 1259 OID 30843)
-- Name: supplies; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.supplies (
    id integer NOT NULL,
    supplier_id integer NOT NULL,
    supply_date date DEFAULT CURRENT_DATE NOT NULL,
    total_amount numeric(10,2),
    notes text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT supplies_total_amount_check CHECK ((total_amount >= (0)::numeric))
);


ALTER TABLE public3.supplies OWNER TO postgres;

--
-- TOC entry 313 (class 1259 OID 30842)
-- Name: supplies_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.supplies_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.supplies_id_seq OWNER TO postgres;

--
-- TOC entry 3668 (class 0 OID 0)
-- Dependencies: 313
-- Name: supplies_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.supplies_id_seq OWNED BY public3.supplies.id;


--
-- TOC entry 316 (class 1259 OID 30864)
-- Name: supply_items; Type: TABLE; Schema: public3; Owner: postgres
--

CREATE TABLE public3.supply_items (
    id integer NOT NULL,
    supply_id integer NOT NULL,
    product_id integer NOT NULL,
    quantity integer NOT NULL,
    purchase_price numeric(10,2),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT supply_items_purchase_price_check CHECK ((purchase_price >= (0)::numeric)),
    CONSTRAINT supply_items_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public3.supply_items OWNER TO postgres;

--
-- TOC entry 315 (class 1259 OID 30863)
-- Name: supply_items_id_seq; Type: SEQUENCE; Schema: public3; Owner: postgres
--

CREATE SEQUENCE public3.supply_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public3.supply_items_id_seq OWNER TO postgres;

--
-- TOC entry 3669 (class 0 OID 0)
-- Dependencies: 315
-- Name: supply_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public3; Owner: postgres
--

ALTER SEQUENCE public3.supply_items_id_seq OWNED BY public3.supply_items.id;


--
-- TOC entry 3348 (class 2604 OID 30712)
-- Name: cart_items id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.cart_items ALTER COLUMN id SET DEFAULT nextval('public3.cart_items_id_seq'::regclass);


--
-- TOC entry 3345 (class 2604 OID 30696)
-- Name: carts id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.carts ALTER COLUMN id SET DEFAULT nextval('public3.carts_id_seq'::regclass);


--
-- TOC entry 3327 (class 2604 OID 30590)
-- Name: categories id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.categories ALTER COLUMN id SET DEFAULT nextval('public3.categories_id_seq'::regclass);


--
-- TOC entry 3340 (class 2604 OID 30653)
-- Name: customers id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.customers ALTER COLUMN id SET DEFAULT nextval('public3.customers_id_seq'::regclass);


--
-- TOC entry 3343 (class 2604 OID 30674)
-- Name: favorites id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.favorites ALTER COLUMN id SET DEFAULT nextval('public3.favorites_id_seq'::regclass);


--
-- TOC entry 3361 (class 2604 OID 30812)
-- Name: notifications id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.notifications ALTER COLUMN id SET DEFAULT nextval('public3.notifications_id_seq'::regclass);


--
-- TOC entry 3354 (class 2604 OID 30760)
-- Name: order_items id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_items ALTER COLUMN id SET DEFAULT nextval('public3.order_items_id_seq'::regclass);


--
-- TOC entry 3331 (class 2604 OID 30613)
-- Name: order_statuses id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_statuses ALTER COLUMN id SET DEFAULT nextval('public3.order_statuses_id_seq'::regclass);


--
-- TOC entry 3350 (class 2604 OID 30733)
-- Name: orders id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.orders ALTER COLUMN id SET DEFAULT nextval('public3.orders_id_seq'::regclass);


--
-- TOC entry 3334 (class 2604 OID 30626)
-- Name: products id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.products ALTER COLUMN id SET DEFAULT nextval('public3.products_id_seq'::regclass);


--
-- TOC entry 3357 (class 2604 OID 30783)
-- Name: reviews id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.reviews ALTER COLUMN id SET DEFAULT nextval('public3.reviews_id_seq'::regclass);


--
-- TOC entry 3330 (class 2604 OID 30604)
-- Name: roles roleid; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.roles ALTER COLUMN roleid SET DEFAULT nextval('public3.roles_roleid_seq'::regclass);


--
-- TOC entry 3364 (class 2604 OID 30831)
-- Name: suppliers id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.suppliers ALTER COLUMN id SET DEFAULT nextval('public3.suppliers_id_seq'::regclass);


--
-- TOC entry 3367 (class 2604 OID 30846)
-- Name: supplies id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supplies ALTER COLUMN id SET DEFAULT nextval('public3.supplies_id_seq'::regclass);


--
-- TOC entry 3371 (class 2604 OID 30867)
-- Name: supply_items id; Type: DEFAULT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supply_items ALTER COLUMN id SET DEFAULT nextval('public3.supply_items_id_seq'::regclass);


--
-- TOC entry 3635 (class 0 OID 30709)
-- Dependencies: 302
-- Data for Name: cart_items; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.cart_items VALUES (36, 3, 3, 1, '2026-04-16 14:38:07.832318');
INSERT INTO public3.cart_items VALUES (37, 3, 5, 1, '2026-04-16 14:38:10.001901');


--
-- TOC entry 3633 (class 0 OID 30693)
-- Dependencies: 300
-- Data for Name: carts; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.carts VALUES (3, 8, '2026-04-16 12:47:54.548479', '2026-04-16 12:47:54.548479');


--
-- TOC entry 3621 (class 0 OID 30587)
-- Dependencies: 288
-- Data for Name: categories; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.categories VALUES (1, 'Единичный цветок', 'Обычный цветок из которого можно собрать букет', '2026-04-04 14:17:55.067', '2026-04-04 14:17:55.067');
INSERT INTO public3.categories VALUES (2, 'Букет', 'Букет из цветов', '2026-04-04 14:17:55.071', '2026-04-04 14:17:55.071');
INSERT INTO public3.categories VALUES (3, 'Корзиночный букет', 'Букет в корзинке', '2026-04-04 14:17:55.071', '2026-04-04 14:17:55.071');
INSERT INTO public3.categories VALUES (4, 'Тест', 'тест', '2026-04-05 17:50:42.088788', '2026-04-05 17:50:42.088788');


--
-- TOC entry 3629 (class 0 OID 30650)
-- Dependencies: 296
-- Data for Name: customers; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.customers VALUES (8, 'Иванов Иван Иванович', 'test@gmail.com', '+79321131245', 1, 'ул. Домодедова, д. 5, кв. 14', '$2a$11$lgMm0EMfEDDXOa3DhhBIyeAPyCceefiJpIH/qCtdYKW/dCpptCr0W', 'Санкт-Петербург', 'avatar_8_9e51923751624e2db1c54fde9b6c00cc.jpg', '2026-04-16 10:37:56.918705', '2026-04-16 14:43:17.431764');
INSERT INTO public3.customers VALUES (5, 'Климко Сергей Викторович', 'test2@gmail.com', '+79313345245', 2, 'ул. Деревенская, д. 10, кв. 144', '$2a$11$GSBGyrndOC8XT/LFNobIg.rKjWPIGL5h/6Vm.XN3eMdGgc91b5XyC', 'Санкт-Петербург', NULL, '2026-04-06 15:03:41.612232', '2026-04-16 14:50:43.749662');


--
-- TOC entry 3631 (class 0 OID 30671)
-- Dependencies: 298
-- Data for Name: favorites; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.favorites VALUES (18, 5, 3, '2026-04-06 15:05:56.782975');
INSERT INTO public3.favorites VALUES (19, 5, 6, '2026-04-06 15:05:59.266621');


--
-- TOC entry 3643 (class 0 OID 30809)
-- Dependencies: 310
-- Data for Name: notifications; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.notifications VALUES (6, 5, 'Информация', 'Ваш заказ доставлен в пункт выдачи', false, '2026-04-16 15:41:38.557405', NULL);
INSERT INTO public3.notifications VALUES (7, 8, 'Информация', 'Ваш заказ был доставлен!', true, '2026-04-16 15:42:02.538886', NULL);


--
-- TOC entry 3639 (class 0 OID 30757)
-- Dependencies: 306
-- Data for Name: order_items; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.order_items VALUES (19, 11, 4, 1, 150.00, 0, '2026-04-16 12:54:34.842775');


--
-- TOC entry 3625 (class 0 OID 30610)
-- Dependencies: 292
-- Data for Name: order_statuses; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.order_statuses VALUES (1, 'Ожидает оплаты', 'Заказ создан, ожидает подтверждения оплаты', 1, '2026-04-04 18:35:51.232303');
INSERT INTO public3.order_statuses VALUES (2, 'Оплачен', 'Оплата получена, заказ готовится к отправке', 2, '2026-04-04 18:35:51.232303');
INSERT INTO public3.order_statuses VALUES (3, 'В доставке', 'Заказ передан в службу доставки', 3, '2026-04-04 18:35:51.232303');
INSERT INTO public3.order_statuses VALUES (4, 'Доставлен', 'Заказ вручён покупателю', 4, '2026-04-04 18:35:51.232303');
INSERT INTO public3.order_statuses VALUES (5, 'Отменён', 'Заказ отменён', 5, '2026-04-04 18:35:51.232303');
INSERT INTO public3.order_statuses VALUES (6, 'Возврат', 'Оформлен возврат товара', 6, '2026-04-04 18:35:51.232303');


--
-- TOC entry 3637 (class 0 OID 30730)
-- Dependencies: 304
-- Data for Name: orders; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.orders VALUES (10, 8, '2026-04-16 12:48:11.493423', 5, 5400.00, ' ул. Пушкинская, д. 10, кв. 335', 'Санкт-Петербург', 'Спасибо за покупку!', '2026-04-16 12:48:11.531816', '2026-04-16 14:36:53.490246');
INSERT INTO public3.orders VALUES (11, 8, '2026-04-16 12:54:34.701717', 4, 150.00, ' ул. Тверская, д. 2, кв. 3', 'Санкт-Петербург', 'Спасибо за покупку!', '2026-04-16 12:54:34.788929', '2026-04-16 14:36:53.490947');


--
-- TOC entry 3627 (class 0 OID 30623)
-- Dependencies: 294
-- Data for Name: products; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.products VALUES (3, 2, 'Розы красные', 'ф', 700.00, 10, '2026-04-06 00:00:00', '2026-04-15 00:00:00', 80, '11.jpg', true, '2026-04-04 14:21:00', '2026-04-16 12:50:26.347012');
INSERT INTO public3.products VALUES (4, 1, 'Гербера ', 'ф', 150.00, 0, '2026-04-04 00:00:00', NULL, 48, '12.jpg', true, '2026-04-04 14:21:00', '2026-04-16 12:54:34.842775');
INSERT INTO public3.products VALUES (5, 2, 'Сборный букет цветов', 'Букет собранный из душистых цветов в едином в оттенках фиолетового и розового', 2000.00, 2, '2026-04-04 00:00:00', '2026-04-25 00:00:00', 2, '13.jpg', true, '2026-04-04 14:21:00', '2026-04-16 14:14:42.732892');
INSERT INTO public3.products VALUES (6, 2, 'Летний букет', '', 3500.00, 35, '2026-04-05 00:00:00', '2026-04-21 00:00:00', 34, '14.jpg', true, '2026-04-04 14:21:00', '2026-04-16 10:39:55.713521');
INSERT INTO public3.products VALUES (1, 1, 'tovar_0', 'товар с 0 количеством на складе', 222.00, 10, '2026-04-07 00:00:00', '2026-05-07 00:00:00', 0, 'd0226b3d31ce4deeb9707a2abed1df53.jpg', true, '2026-04-05 18:23:54.884904', '2026-04-16 10:48:38.481194');


--
-- TOC entry 3641 (class 0 OID 30780)
-- Dependencies: 308
-- Data for Name: reviews; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.reviews VALUES (8, 4, 8, 5, NULL, false, '2026-04-16 12:56:54.351069', '2026-04-16 12:56:59.461676');
INSERT INTO public3.reviews VALUES (11, 5, 8, 5, 'Потрясающий букет', false, '2026-04-16 14:15:03.600179', '2026-04-16 14:15:03.675201');


--
-- TOC entry 3623 (class 0 OID 30601)
-- Dependencies: 290
-- Data for Name: roles; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.roles VALUES (1, 'Администратор');
INSERT INTO public3.roles VALUES (2, 'Пользователь');
INSERT INTO public3.roles VALUES (3, 'Менеджер');


--
-- TOC entry 3645 (class 0 OID 30828)
-- Dependencies: 312
-- Data for Name: suppliers; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.suppliers VALUES (8, 'ООО Розы', 'Юрьев Андрей Михайлович', '+79231321234', 'test@gmail.com', 'ул. Домодедова, д.7, кв 5', 'Санкт-Петербург', '2026-04-05 21:32:22.418941', '2026-04-16 15:20:22.377465');
INSERT INTO public3.suppliers VALUES (1, 'ООО Роса', 'Иванов Иван Иваныч', '+77777777777', 'test@yandex.ru', 'ул. Бульвар д.7 кв. 145', 'Москва', '2026-04-05 00:35:05.136585', '2026-04-16 15:20:27.729267');


--
-- TOC entry 3647 (class 0 OID 30843)
-- Dependencies: 314
-- Data for Name: supplies; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.supplies VALUES (1, 1, '2027-04-05', 15000.00, 'Заказ роз', '2026-04-05 01:04:23.175245', '2026-04-05 13:35:18.24737');
INSERT INTO public3.supplies VALUES (4, 1, '2027-04-06', 4640.00, NULL, '2026-04-05 13:06:23.664843', '2026-04-05 20:27:05.233273');
INSERT INTO public3.supplies VALUES (6, 1, '2026-04-05', 444.00, NULL, '2026-04-05 21:09:42.606959', '2026-04-05 21:09:42.606959');
INSERT INTO public3.supplies VALUES (8, 8, '2026-04-06', 16716.00, 'авалвоваловолт', '2026-04-05 21:40:50.099798', '2026-04-05 21:40:50.099798');


--
-- TOC entry 3649 (class 0 OID 30864)
-- Dependencies: 316
-- Data for Name: supply_items; Type: TABLE DATA; Schema: public3; Owner: postgres
--

INSERT INTO public3.supply_items VALUES (10, 1, 3, 10, 1500.00, '2026-04-05 13:35:18.289506');
INSERT INTO public3.supply_items VALUES (11, 4, 6, 20, 132.00, '2026-04-05 20:27:05.263622');
INSERT INTO public3.supply_items VALUES (12, 4, 1, 10, 200.00, '2026-04-05 20:27:05.263622');
INSERT INTO public3.supply_items VALUES (15, 6, 1, 2, 222.00, '2026-04-05 21:09:42.635414');
INSERT INTO public3.supply_items VALUES (17, 8, 4, 134, 124.00, '2026-04-05 21:40:50.142018');
INSERT INTO public3.supply_items VALUES (18, 8, 1, 1, 100.00, '2026-04-05 21:40:50.142018');


--
-- TOC entry 3670 (class 0 OID 0)
-- Dependencies: 301
-- Name: cart_items_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.cart_items_id_seq', 37, true);


--
-- TOC entry 3671 (class 0 OID 0)
-- Dependencies: 299
-- Name: carts_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.carts_id_seq', 3, true);


--
-- TOC entry 3672 (class 0 OID 0)
-- Dependencies: 287
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.categories_id_seq', 1, false);


--
-- TOC entry 3673 (class 0 OID 0)
-- Dependencies: 295
-- Name: customers_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.customers_id_seq', 8, true);


--
-- TOC entry 3674 (class 0 OID 0)
-- Dependencies: 297
-- Name: favorites_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.favorites_id_seq', 21, true);


--
-- TOC entry 3675 (class 0 OID 0)
-- Dependencies: 309
-- Name: notifications_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.notifications_id_seq', 7, true);


--
-- TOC entry 3676 (class 0 OID 0)
-- Dependencies: 305
-- Name: order_items_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.order_items_id_seq', 19, true);


--
-- TOC entry 3677 (class 0 OID 0)
-- Dependencies: 291
-- Name: order_statuses_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.order_statuses_id_seq', 6, true);


--
-- TOC entry 3678 (class 0 OID 0)
-- Dependencies: 303
-- Name: orders_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.orders_id_seq', 11, true);


--
-- TOC entry 3679 (class 0 OID 0)
-- Dependencies: 293
-- Name: products_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.products_id_seq', 4, true);


--
-- TOC entry 3680 (class 0 OID 0)
-- Dependencies: 307
-- Name: reviews_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.reviews_id_seq', 11, true);


--
-- TOC entry 3681 (class 0 OID 0)
-- Dependencies: 289
-- Name: roles_roleid_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.roles_roleid_seq', 1, false);


--
-- TOC entry 3682 (class 0 OID 0)
-- Dependencies: 311
-- Name: suppliers_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.suppliers_id_seq', 12, true);


--
-- TOC entry 3683 (class 0 OID 0)
-- Dependencies: 313
-- Name: supplies_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.supplies_id_seq', 10, true);


--
-- TOC entry 3684 (class 0 OID 0)
-- Dependencies: 315
-- Name: supply_items_id_seq; Type: SEQUENCE SET; Schema: public3; Owner: postgres
--

SELECT pg_catalog.setval('public3.supply_items_id_seq', 24, true);


--
-- TOC entry 3416 (class 2606 OID 30716)
-- Name: cart_items cart_items_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.cart_items
    ADD CONSTRAINT cart_items_pkey PRIMARY KEY (id);


--
-- TOC entry 3413 (class 2606 OID 30700)
-- Name: carts carts_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.carts
    ADD CONSTRAINT carts_pkey PRIMARY KEY (id);


--
-- TOC entry 3385 (class 2606 OID 30598)
-- Name: categories categories_name_key; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.categories
    ADD CONSTRAINT categories_name_key UNIQUE (name);


--
-- TOC entry 3387 (class 2606 OID 30596)
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- TOC entry 3401 (class 2606 OID 30661)
-- Name: customers customers_email_key; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.customers
    ADD CONSTRAINT customers_email_key UNIQUE (email);


--
-- TOC entry 3403 (class 2606 OID 30659)
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (id);


--
-- TOC entry 3407 (class 2606 OID 30679)
-- Name: favorites favorites_customer_id_product_id_key; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.favorites
    ADD CONSTRAINT favorites_customer_id_product_id_key UNIQUE (customer_id, product_id);


--
-- TOC entry 3409 (class 2606 OID 30677)
-- Name: favorites favorites_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.favorites
    ADD CONSTRAINT favorites_pkey PRIMARY KEY (id);


--
-- TOC entry 3439 (class 2606 OID 30818)
-- Name: notifications notifications_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.notifications
    ADD CONSTRAINT notifications_pkey PRIMARY KEY (id);


--
-- TOC entry 3427 (class 2606 OID 30766)
-- Name: order_items order_items_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_items
    ADD CONSTRAINT order_items_pkey PRIMARY KEY (id);


--
-- TOC entry 3391 (class 2606 OID 30621)
-- Name: order_statuses order_statuses_name_key; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_statuses
    ADD CONSTRAINT order_statuses_name_key UNIQUE (name);


--
-- TOC entry 3393 (class 2606 OID 30619)
-- Name: order_statuses order_statuses_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_statuses
    ADD CONSTRAINT order_statuses_pkey PRIMARY KEY (id);


--
-- TOC entry 3423 (class 2606 OID 30741)
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);


--
-- TOC entry 3399 (class 2606 OID 30638)
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


--
-- TOC entry 3432 (class 2606 OID 30791)
-- Name: reviews reviews_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.reviews
    ADD CONSTRAINT reviews_pkey PRIMARY KEY (id);


--
-- TOC entry 3434 (class 2606 OID 30793)
-- Name: reviews reviews_product_id_customer_id_key; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.reviews
    ADD CONSTRAINT reviews_product_id_customer_id_key UNIQUE (product_id, customer_id);


--
-- TOC entry 3389 (class 2606 OID 30608)
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (roleid);


--
-- TOC entry 3442 (class 2606 OID 30839)
-- Name: suppliers suppliers_name_key; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.suppliers
    ADD CONSTRAINT suppliers_name_key UNIQUE (name);


--
-- TOC entry 3444 (class 2606 OID 30837)
-- Name: suppliers suppliers_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.suppliers
    ADD CONSTRAINT suppliers_pkey PRIMARY KEY (id);


--
-- TOC entry 3448 (class 2606 OID 30854)
-- Name: supplies supplies_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supplies
    ADD CONSTRAINT supplies_pkey PRIMARY KEY (id);


--
-- TOC entry 3452 (class 2606 OID 30872)
-- Name: supply_items supply_items_pkey; Type: CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supply_items
    ADD CONSTRAINT supply_items_pkey PRIMARY KEY (id);


--
-- TOC entry 3417 (class 1259 OID 30727)
-- Name: idx_cart_items_cart_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_cart_items_cart_id ON public3.cart_items USING btree (cart_id);


--
-- TOC entry 3418 (class 1259 OID 30728)
-- Name: idx_cart_items_product_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_cart_items_product_id ON public3.cart_items USING btree (product_id);


--
-- TOC entry 3414 (class 1259 OID 30706)
-- Name: idx_carts_customer_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_carts_customer_id ON public3.carts USING btree (customer_id);


--
-- TOC entry 3404 (class 1259 OID 30667)
-- Name: idx_customers_email; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_customers_email ON public3.customers USING btree (email);


--
-- TOC entry 3405 (class 1259 OID 30668)
-- Name: idx_customers_phone; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_customers_phone ON public3.customers USING btree (phone);


--
-- TOC entry 3410 (class 1259 OID 30690)
-- Name: idx_favorites_customer_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_favorites_customer_id ON public3.favorites USING btree (customer_id);


--
-- TOC entry 3411 (class 1259 OID 30691)
-- Name: idx_favorites_product_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_favorites_product_id ON public3.favorites USING btree (product_id);


--
-- TOC entry 3435 (class 1259 OID 30826)
-- Name: idx_notifications_created_at; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_notifications_created_at ON public3.notifications USING btree (created_at);


--
-- TOC entry 3436 (class 1259 OID 30824)
-- Name: idx_notifications_customer_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_notifications_customer_id ON public3.notifications USING btree (customer_id);


--
-- TOC entry 3437 (class 1259 OID 30825)
-- Name: idx_notifications_is_read; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_notifications_is_read ON public3.notifications USING btree (is_read);


--
-- TOC entry 3424 (class 1259 OID 30777)
-- Name: idx_order_items_order_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_order_items_order_id ON public3.order_items USING btree (order_id);


--
-- TOC entry 3425 (class 1259 OID 30778)
-- Name: idx_order_items_product_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_order_items_product_id ON public3.order_items USING btree (product_id);


--
-- TOC entry 3419 (class 1259 OID 30752)
-- Name: idx_orders_customer_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_orders_customer_id ON public3.orders USING btree (customer_id);


--
-- TOC entry 3420 (class 1259 OID 30753)
-- Name: idx_orders_order_date; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_orders_order_date ON public3.orders USING btree (order_date);


--
-- TOC entry 3421 (class 1259 OID 30754)
-- Name: idx_orders_status_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_orders_status_id ON public3.orders USING btree (status_id);


--
-- TOC entry 3394 (class 1259 OID 30644)
-- Name: idx_products_category_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_products_category_id ON public3.products USING btree (category_id);


--
-- TOC entry 3395 (class 1259 OID 30647)
-- Name: idx_products_discount_dates; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_products_discount_dates ON public3.products USING btree (discount_start_date, discount_end_date) WHERE (discount_percent > 0);


--
-- TOC entry 3396 (class 1259 OID 30646)
-- Name: idx_products_is_active; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_products_is_active ON public3.products USING btree (is_active);


--
-- TOC entry 3397 (class 1259 OID 30645)
-- Name: idx_products_name; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_products_name ON public3.products USING btree (name);


--
-- TOC entry 3428 (class 1259 OID 30805)
-- Name: idx_reviews_customer_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_reviews_customer_id ON public3.reviews USING btree (customer_id);


--
-- TOC entry 3429 (class 1259 OID 30804)
-- Name: idx_reviews_product_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_reviews_product_id ON public3.reviews USING btree (product_id);


--
-- TOC entry 3430 (class 1259 OID 30806)
-- Name: idx_reviews_rating; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_reviews_rating ON public3.reviews USING btree (rating);


--
-- TOC entry 3440 (class 1259 OID 30840)
-- Name: idx_suppliers_name; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_suppliers_name ON public3.suppliers USING btree (name);


--
-- TOC entry 3445 (class 1259 OID 30860)
-- Name: idx_supplies_supplier_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_supplies_supplier_id ON public3.supplies USING btree (supplier_id);


--
-- TOC entry 3446 (class 1259 OID 30861)
-- Name: idx_supplies_supply_date; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_supplies_supply_date ON public3.supplies USING btree (supply_date);


--
-- TOC entry 3449 (class 1259 OID 30884)
-- Name: idx_supply_items_product_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_supply_items_product_id ON public3.supply_items USING btree (product_id);


--
-- TOC entry 3450 (class 1259 OID 30883)
-- Name: idx_supply_items_supply_id; Type: INDEX; Schema: public3; Owner: postgres
--

CREATE INDEX idx_supply_items_supply_id ON public3.supply_items USING btree (supply_id);


--
-- TOC entry 3473 (class 2620 OID 30707)
-- Name: carts trigger_carts_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_carts_updated_at BEFORE UPDATE ON public3.carts FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3470 (class 2620 OID 30599)
-- Name: categories trigger_categories_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_categories_updated_at BEFORE UPDATE ON public3.categories FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3472 (class 2620 OID 30669)
-- Name: customers trigger_customers_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_customers_updated_at BEFORE UPDATE ON public3.customers FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3474 (class 2620 OID 30755)
-- Name: orders trigger_orders_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_orders_updated_at BEFORE UPDATE ON public3.orders FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3471 (class 2620 OID 30648)
-- Name: products trigger_products_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_products_updated_at BEFORE UPDATE ON public3.products FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3475 (class 2620 OID 30807)
-- Name: reviews trigger_reviews_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_reviews_updated_at BEFORE UPDATE ON public3.reviews FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3476 (class 2620 OID 30841)
-- Name: suppliers trigger_suppliers_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_suppliers_updated_at BEFORE UPDATE ON public3.suppliers FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3477 (class 2620 OID 30862)
-- Name: supplies trigger_supplies_updated_at; Type: TRIGGER; Schema: public3; Owner: postgres
--

CREATE TRIGGER trigger_supplies_updated_at BEFORE UPDATE ON public3.supplies FOR EACH ROW EXECUTE FUNCTION public3.update_updated_at_column();


--
-- TOC entry 3458 (class 2606 OID 30717)
-- Name: cart_items cart_items_cart_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.cart_items
    ADD CONSTRAINT cart_items_cart_id_fkey FOREIGN KEY (cart_id) REFERENCES public3.carts(id) ON DELETE CASCADE;


--
-- TOC entry 3459 (class 2606 OID 30722)
-- Name: cart_items cart_items_product_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.cart_items
    ADD CONSTRAINT cart_items_product_id_fkey FOREIGN KEY (product_id) REFERENCES public3.products(id) ON DELETE RESTRICT;


--
-- TOC entry 3457 (class 2606 OID 30701)
-- Name: carts carts_customer_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.carts
    ADD CONSTRAINT carts_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public3.customers(id) ON DELETE CASCADE;


--
-- TOC entry 3454 (class 2606 OID 30662)
-- Name: customers customers_roleid_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.customers
    ADD CONSTRAINT customers_roleid_fkey FOREIGN KEY (roleid) REFERENCES public3.roles(roleid);


--
-- TOC entry 3455 (class 2606 OID 30680)
-- Name: favorites favorites_customer_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.favorites
    ADD CONSTRAINT favorites_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public3.customers(id) ON DELETE CASCADE;


--
-- TOC entry 3456 (class 2606 OID 30685)
-- Name: favorites favorites_product_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.favorites
    ADD CONSTRAINT favorites_product_id_fkey FOREIGN KEY (product_id) REFERENCES public3.products(id) ON DELETE CASCADE;


--
-- TOC entry 3466 (class 2606 OID 30819)
-- Name: notifications notifications_customer_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.notifications
    ADD CONSTRAINT notifications_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public3.customers(id) ON DELETE CASCADE;


--
-- TOC entry 3462 (class 2606 OID 30767)
-- Name: order_items order_items_order_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_items
    ADD CONSTRAINT order_items_order_id_fkey FOREIGN KEY (order_id) REFERENCES public3.orders(id) ON DELETE CASCADE;


--
-- TOC entry 3463 (class 2606 OID 30772)
-- Name: order_items order_items_product_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.order_items
    ADD CONSTRAINT order_items_product_id_fkey FOREIGN KEY (product_id) REFERENCES public3.products(id) ON DELETE RESTRICT;


--
-- TOC entry 3460 (class 2606 OID 30742)
-- Name: orders orders_customer_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.orders
    ADD CONSTRAINT orders_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public3.customers(id) ON DELETE RESTRICT;


--
-- TOC entry 3461 (class 2606 OID 30747)
-- Name: orders orders_status_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.orders
    ADD CONSTRAINT orders_status_id_fkey FOREIGN KEY (status_id) REFERENCES public3.order_statuses(id);


--
-- TOC entry 3453 (class 2606 OID 30639)
-- Name: products products_category_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.products
    ADD CONSTRAINT products_category_id_fkey FOREIGN KEY (category_id) REFERENCES public3.categories(id) ON DELETE RESTRICT;


--
-- TOC entry 3464 (class 2606 OID 30799)
-- Name: reviews reviews_customer_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.reviews
    ADD CONSTRAINT reviews_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public3.customers(id) ON DELETE CASCADE;


--
-- TOC entry 3465 (class 2606 OID 30794)
-- Name: reviews reviews_product_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.reviews
    ADD CONSTRAINT reviews_product_id_fkey FOREIGN KEY (product_id) REFERENCES public3.products(id) ON DELETE CASCADE;


--
-- TOC entry 3467 (class 2606 OID 30855)
-- Name: supplies supplies_supplier_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supplies
    ADD CONSTRAINT supplies_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public3.suppliers(id) ON DELETE RESTRICT;


--
-- TOC entry 3468 (class 2606 OID 30878)
-- Name: supply_items supply_items_product_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supply_items
    ADD CONSTRAINT supply_items_product_id_fkey FOREIGN KEY (product_id) REFERENCES public3.products(id) ON DELETE RESTRICT;


--
-- TOC entry 3469 (class 2606 OID 30873)
-- Name: supply_items supply_items_supply_id_fkey; Type: FK CONSTRAINT; Schema: public3; Owner: postgres
--

ALTER TABLE ONLY public3.supply_items
    ADD CONSTRAINT supply_items_supply_id_fkey FOREIGN KEY (supply_id) REFERENCES public3.supplies(id) ON DELETE CASCADE;


-- Completed on 2026-04-20 14:28:19

--
-- PostgreSQL database dump complete
--

