using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.PhucLong.API.Queries.POS
{
    public static class PosOrderQuery
    {
        public static string get_pos_order_cancel(string date_orde, int location_id)
        {
            //return @"SELECT s.id, s.name, s.state, s.location_id
            //        FROM public.pos_order s
            //        INNER JOIN public.pos_payment p on s.id = p.pos_order_id 
            //        WHERE s.state = 'cancel' 
            //         AND EXISTS (SELECT 1 FROM public.pos_staging r WHERE s.id = r.order_id AND s.location_id = r.location_id AND r.is_payment = true)
            //            AND p.warehouse_id = " + warehouse_id + @"
            //         AND p.payment_method_id = ANY(@payment_method)
            //         AND Cast(s.date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_orde + @"';"; 

            return @"   select s.*
                        from public.pos_staging s
                        inner join public.pos_order o on s.order_id = o.id and s.state = 'paid'
                        where o.state = 'cancel'
                              AND s.location_id = " + location_id + @"
                              AND cast(o.date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_orde + @"';";
        }
        public static string get_pos_order_all(int location_id, string order_date)
        {
            return @"
                    SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_order, user_id, amount_tax, amount_total, amount_paid, amount_return, pricelist_id, partner_id, state, account_move, picking_id, 
                     location_id, note, pos_reference, sale_journal, to_invoice, create_uid, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  employee_id, cashier, discount_amount, invoice_id, 
                     warehouse_id, cashier_id, coupon_code, promotion_id, picking_return_id, return_origin, loyalty_points, point_won, year_discount_birth, sale_type_id, note_label, disable_loyalty_discount, 
                     has_printed_label_first, linked_draft_order_be, use_emp_coupon, emp_coupon_code, current_coupon_limit, current_coupon_promotion, total_surcharge, number_of_printed_bill,  hanging_time, 
                     reward_code, momo_payment_ref, partner_current_point, partner_total_point, partner_loyalty_level_id, cast(partner_expired_date::timestamp AT TIME ZONE 'UTC' as date) partner_expired_date, auto_paid_by_cron, message_main_attachment_id, 
                     cast(date_last_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_last_order, cancel_from_be, moca_payment_ref, cancel_reason, cancel_duplicate, pay_draft_order, invoice_name, invoice_vat, invoice_address, invoice_email, invoice_contact, invoice_note, invoice_request, 
                     zalo_payment_ref
                    FROM public.pos_order o
                    WHERE NOT EXISTS (SELECT 1 FROM public.pos_staging s WHERE o.id = s.order_id and o.location_id = s.location_id) 
                            AND state = 'paid' 
                            AND location_id = " + location_id + @"
                            AND cast(date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + order_date + @"';";
        }
        public static string get_pos_order_by_order(string orderNo)
        {
            return @"
                    SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_order, user_id, amount_tax, amount_total, amount_paid, amount_return, pricelist_id, partner_id, state, account_move, picking_id, 
                     location_id, note, pos_reference, sale_journal, to_invoice, create_uid, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  employee_id, cashier, discount_amount, invoice_id, 
                     warehouse_id, cashier_id, coupon_code, promotion_id, picking_return_id, return_origin, loyalty_points, point_won, year_discount_birth, sale_type_id, note_label, disable_loyalty_discount, 
                     has_printed_label_first, linked_draft_order_be, use_emp_coupon, emp_coupon_code, current_coupon_limit, current_coupon_promotion, total_surcharge, number_of_printed_bill,  hanging_time, 
                     reward_code, momo_payment_ref, partner_current_point, partner_total_point, partner_loyalty_level_id, cast(partner_expired_date::timestamp AT TIME ZONE 'UTC' as date) partner_expired_date, auto_paid_by_cron, message_main_attachment_id, 
                     cast(date_last_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_last_order, cancel_from_be, moca_payment_ref, cancel_reason, cancel_duplicate, pay_draft_order, invoice_name, invoice_vat, invoice_address, invoice_email, invoice_contact, invoice_note, invoice_request, 
                     zalo_payment_ref
                    FROM public.pos_order o
                    WHERE name = '" + orderNo + @"';";
        }
        public static string get_pos_order()
        {
            return @"
                    SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_order, user_id, amount_tax, amount_total, amount_paid, amount_return, pricelist_id, partner_id, state, account_move, picking_id, 
                     location_id, note, pos_reference, sale_journal, to_invoice, create_uid, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  employee_id, cashier, discount_amount, invoice_id, 
                     warehouse_id, cashier_id, coupon_code, promotion_id, picking_return_id, return_origin, loyalty_points, point_won, year_discount_birth, sale_type_id, note_label, disable_loyalty_discount, 
                     has_printed_label_first, linked_draft_order_be, use_emp_coupon, emp_coupon_code, current_coupon_limit, current_coupon_promotion, total_surcharge, number_of_printed_bill,  hanging_time, 
                     reward_code, momo_payment_ref, partner_current_point, partner_total_point, partner_loyalty_level_id, cast(partner_expired_date::timestamp AT TIME ZONE 'UTC' as date) partner_expired_date, auto_paid_by_cron, message_main_attachment_id, 
                     cast(date_last_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_last_order, cancel_from_be, moca_payment_ref, cancel_reason, cancel_duplicate, pay_draft_order, invoice_name, invoice_vat, invoice_address, invoice_email, invoice_contact, invoice_note, invoice_request, zalo_payment_ref
                    FROM public.pos_order o
                    WHERE id = ANY(@order_id);";
        }
        public static string get_pos_order_line()
        {
            return @"SELECT id, name, product_id, price_unit, qty, price_subtotal, price_subtotal_incl, discount, order_id, create_date, is_promotion_line, is_condition_line, 
                            promotion_id, promotion_condition_id, uom_id, old_price, is_manual_discount, return_discount, loyalty_discount_percent, promotion_line_id, is_loyalty_line, fe_uid,
                            is_birthday_promotion, note, discount_amount,  is_topping_line, related_line_id, disable_promotion, disable_loyalty, partner_id, old_price_total, warehouse_id, date_order,
                            loyalty_point_cost, combo_id, is_done_combo, combo_seq, amount_surcharge, combo_qty, reward_id, cashless_code, product_coupon_code
                            FROM public.pos_order_line
                            WHERE order_id = ANY(@order_id) "; // + order_id + @";";
        }
        public static string get_pos_payment_by_order()
        {
            return @"SELECT id, pos_order_id, amount, payment_method_id, cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) payment_date, card_type, transaction_id, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  voucher_code, warehouse_id, user_id, 
                        cashier_id, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state_pos, employee_id, currency_name, currency_origin_value, exchange_rate, on_account_info, mercury_card_number, mercury_card_brand, 
                        mercury_card_owner_name, mercury_ref_no, mercury_record_no, mercury_invoice_no, amount_change, voucher_max_value, partner_id, on_account_partner_id
                    FROM public.pos_payment o
                    WHERE pos_order_id = ANY(@order_id)";
        }
        public static string get_pos_payment(string payment_date, int warehouse_id, int[] payment_method)
        {
            //if(payment_method.ToList().Count > 0)
            //{
            //    return @"SELECT id, pos_order_id, amount, payment_method_id, cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) payment_date, card_type, transaction_id, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  voucher_code, warehouse_id, user_id, 
            //            cashier_id, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state_pos, employee_id, currency_name, currency_origin_value, exchange_rate, on_account_info, mercury_card_number, mercury_card_brand, 
            //            mercury_card_owner_name, mercury_ref_no, mercury_record_no, mercury_invoice_no, amount_change, voucher_max_value, partner_id, on_account_partner_id
            //        FROM public.pos_payment o
            //        WHERE NOT EXISTS (SELECT 1 FROM public.pos_staging r WHERE o.pos_order_id = r.order_id) 
            //                AND state_pos = 'paid' 
            //                AND warehouse_id = " + warehouse_id + @"
            //                AND cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) >= '" + payment_date + @"' AND payment_method_id = ANY(@payment_method);";
            //}
            //else 
            //{
            //    return @"SELECT id, pos_order_id, amount, payment_method_id, cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) payment_date, card_type, transaction_id, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  voucher_code, warehouse_id, user_id, 
            //            cashier_id, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state_pos, employee_id, currency_name, currency_origin_value, exchange_rate, on_account_info, mercury_card_number, mercury_card_brand, 
            //            mercury_card_owner_name, mercury_ref_no, mercury_record_no, mercury_invoice_no, amount_change, voucher_max_value, partner_id, on_account_partner_id
            //        FROM public.pos_payment o
            //        WHERE NOT EXISTS (SELECT 1 FROM public.pos_staging r WHERE o.pos_order_id = r.order_id) 
            //                AND state_pos = 'paid' 
            //                AND warehouse_id = " + warehouse_id + @"
            //                AND cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) >= '" + payment_date + @"';";
            //}

            return @"SELECT id, pos_order_id, amount, payment_method_id, cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) payment_date, card_type, transaction_id, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  voucher_code, warehouse_id, user_id, 
	                       cashier_id, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state_pos, employee_id, currency_name, currency_origin_value, exchange_rate, on_account_info, mercury_card_number, mercury_card_brand, 
	                       mercury_card_owner_name, mercury_ref_no, mercury_record_no, mercury_invoice_no, amount_change, voucher_max_value, partner_id, on_account_partner_id
                    FROM public.pos_payment o
                    WHERE NOT EXISTS (SELECT 1 FROM public.pos_staging r WHERE o.pos_order_id = r.order_id) 
                            AND state_pos = 'paid' 
                            AND warehouse_id = " + warehouse_id + @"
                            AND cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) >= '" + payment_date + @"';";

            //return @"SELECT id, pos_order_id, amount, payment_method_id, cast(payment_date::timestamp AT TIME ZONE 'UTC' as date) payment_date, card_type, transaction_id, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  voucher_code, warehouse_id, user_id, 
            //            cashier_id, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state_pos, employee_id, currency_name, currency_origin_value, exchange_rate, on_account_info, mercury_card_number, mercury_card_brand, 
            //            mercury_card_owner_name, mercury_ref_no, mercury_record_no, mercury_invoice_no, amount_change, voucher_max_value, partner_id, on_account_partner_id
            //        FROM public.pos_payment o
            //        WHERE pos_order_id = 76313";
        }
    }
}
