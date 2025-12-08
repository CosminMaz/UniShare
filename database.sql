-- PostgreSQL database import script for UniShare
-- Cleaned and ready for Docker

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';
SET default_table_access_method = heap;

-- Required for gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

------------------------------------------------------
-- TABLE: __EFMigrationsHistory
------------------------------------------------------

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);

ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

------------------------------------------------------
-- TABLE: users
------------------------------------------------------

CREATE TABLE public.users (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    full_name text NOT NULL,
    email text NOT NULL,
    password_hash text NOT NULL,
    role integer NOT NULL,
    created_at timestamp with time zone NOT NULL
);

ALTER TABLE public.users OWNER TO postgres;

------------------------------------------------------
-- TABLE: items
------------------------------------------------------

CREATE TABLE public.items (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    owner_id uuid NOT NULL,
    title text NOT NULL,
    description text NOT NULL,
    category text NOT NULL,
    condition text NOT NULL,
    daily_rate numeric(10,2),
    image_url text,
    is_available boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT items_category_check CHECK ((category = ANY (ARRAY['Books','Electronics','Clothing','Furniture','Sports','Other']))),
    CONSTRAINT items_condition_check CHECK ((condition = ANY (ARRAY['New','LikeNew','WellPreserved','Acceptable','Poor'])))
);

ALTER TABLE public.items OWNER TO postgres;

------------------------------------------------------
-- TABLE: bookings
------------------------------------------------------

CREATE TABLE public.bookings (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    item_id uuid NOT NULL,
    borrower_id uuid NOT NULL,
    owner_id uuid NOT NULL,
    status text NOT NULL CHECK (status IN ('Pending','Approved','Active','Completed','Cancelled','Rejected')),
    start_date timestamp with time zone NOT NULL,
    end_date timestamp with time zone NOT NULL,
    actual_return_date timestamp with time zone,
    total_price numeric(10,2),
    requested_at timestamp with time zone NOT NULL,
    approved_at timestamp with time zone,
    completed_at timestamp with time zone
);

ALTER TABLE public.bookings OWNER TO postgres;

------------------------------------------------------
-- TABLE: reviews
------------------------------------------------------

CREATE TABLE public.reviews (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    booking_id uuid NOT NULL,
    reviewer_id uuid NOT NULL,
    item_id uuid NOT NULL,
    rating integer NOT NULL,
    comment text,
    review_type integer NOT NULL,
    created_at timestamp with time zone NOT NULL
);

ALTER TABLE public.reviews OWNER TO postgres;

------------------------------------------------------
-- CONSTRAINTS
------------------------------------------------------

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.reviews
    ADD CONSTRAINT reviews_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(id);

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_borrower_id_fkey FOREIGN KEY (borrower_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.reviews
    ADD CONSTRAINT reviews_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(id);

ALTER TABLE ONLY public.reviews
    ADD CONSTRAINT reviews_reviewer_id_fkey FOREIGN KEY (reviewer_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.reviews
    ADD CONSTRAINT reviews_booking_id_fkey FOREIGN KEY (booking_id) REFERENCES public.bookings(id);

------------------------------------------------------
-- DONE 🚀
------------------------------------------------------
