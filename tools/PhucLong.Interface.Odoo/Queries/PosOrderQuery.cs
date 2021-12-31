using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Odoo.Queries
{
    public static class PosOrderQuery
    {
        public static string StrQueryTransHeader(string date_order, int limit_top, bool isHistoric = true)
        {
            if (isHistoric)
            {
                return @"
                    SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, user_id, amount_tax, amount_total, amount_paid, amount_return, pricelist_id, partner_id, state, account_move, picking_id, 
                     location_id, note, pos_reference, sale_journal, to_invoice, create_uid, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  employee_id, cashier, discount_amount, invoice_id, 
                     warehouse_id, cashier_id, coupon_code, promotion_id, picking_return_id, return_origin, loyalty_points, point_won, year_discount_birth, sale_type_id, note_label, disable_loyalty_discount, 
                     has_printed_label_first, linked_draft_order_be, use_emp_coupon, emp_coupon_code, current_coupon_limit, current_coupon_promotion, total_surcharge, number_of_printed_bill,  hanging_time, 
                     reward_code, momo_payment_ref, partner_current_point, partner_total_point, partner_loyalty_level_id, cast(partner_expired_date::timestamp AT TIME ZONE 'UTC' as date) partner_expired_date, auto_paid_by_cron, message_main_attachment_id, 
                     cast(date_last_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_last_order, cancel_from_be, moca_payment_ref, cancel_reason, cancel_duplicate, pay_draft_order, invoice_name, invoice_vat, invoice_address, invoice_email, invoice_contact, invoice_note, invoice_request, zalo_payment_ref,
                     mobile_receiver_info, partner_insert_type, order_in_call_center, session_callcenter_id, cv_life_redeem, cv_life_earn
                    FROM public.pos_order o
                    WHERE NOT EXISTS (SELECT 1 FROM public.pos_raw r WHERE o.id = r.order_id AND o.location_id = r.location_id) 
                          AND state = 'paid' and Cast(date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_order + @"' 
                    ORDER BY date_order LIMIT(" + limit_top + @");";
            }
            else
            {
                return @"
                    SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, user_id, amount_tax, amount_total, amount_paid, amount_return, pricelist_id, partner_id, state, account_move, picking_id, 
                     location_id, note, pos_reference, sale_journal, to_invoice, create_uid, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,  employee_id, cashier, discount_amount, invoice_id, 
                     warehouse_id, cashier_id, coupon_code, promotion_id, picking_return_id, return_origin, loyalty_points, point_won, year_discount_birth, sale_type_id, note_label, disable_loyalty_discount, 
                     has_printed_label_first, linked_draft_order_be, use_emp_coupon, emp_coupon_code, current_coupon_limit, current_coupon_promotion, total_surcharge, number_of_printed_bill,  hanging_time, 
                     reward_code, momo_payment_ref, partner_current_point, partner_total_point, partner_loyalty_level_id, cast(partner_expired_date::timestamp AT TIME ZONE 'UTC' as date) partner_expired_date, auto_paid_by_cron, message_main_attachment_id, 
                     cast(date_last_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_last_order, cancel_from_be, moca_payment_ref, cancel_reason, cancel_duplicate, pay_draft_order, invoice_name, invoice_vat, invoice_address, invoice_email, invoice_contact, invoice_note, invoice_request, zalo_payment_ref,
                     mobile_receiver_info, partner_insert_type, order_in_call_center, session_callcenter_id, cv_life_redeem, cv_life_earn
                    FROM public.pos_order
                    WHERE location_id = 532 AND state = 'paid' and Cast(date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_order + @"' 
                    ORDER BY date_order LIMIT(" + limit_top + @");";
            }
        }
        public static string StrQueryTransLine()
        {
            return @"SELECT id, name, product_id, price_unit, qty, price_subtotal, price_subtotal_incl, discount, order_id, create_date, is_promotion_line, is_condition_line, 
                            promotion_id, promotion_condition_id, uom_id, old_price, is_manual_discount, return_discount, loyalty_discount_percent, promotion_line_id, is_loyalty_line, 
                            is_birthday_promotion, note, discount_amount,  is_topping_line, related_line_id, disable_promotion, disable_loyalty, partner_id, old_price_total, warehouse_id, date_order,
                            loyalty_point_cost, combo_id, is_done_combo, combo_seq, amount_surcharge, combo_qty, reward_id, cashless_code, product_coupon_code, cup_type, fe_uid,
                            code, plh_redeem, plh_earn, cv_life_redeem, cv_life_earn
                            FROM public.pos_order_line
                            WHERE order_id = ANY(@order_id) "; // + order_id + @";";
        }
        public static string StrQueryTransPaymentEntry()
        {
            return @"SELECT id, pos_order_id, amount, payment_method_id, payment_date, card_type, transaction_id, cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date, voucher_code, warehouse_id, user_id, 
	                       cashier_id, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state_pos, employee_id, currency_name, currency_origin_value, exchange_rate, on_account_info, mercury_card_number, mercury_card_brand, 
	                       mercury_card_owner_name, mercury_ref_no, mercury_record_no, mercury_invoice_no, amount_change, voucher_max_value, partner_id, on_account_partner_id
                    FROM public.pos_payment
                    WHERE state_pos = 'paid' and pos_order_id = ANY(@order_id);";
        }
        public static string StrQueryTransLineOptions()
        {
            return @"select a.id, a.line_id, a.option_id, a.option_type, p2.name, p1.product_id, p1.product_uom_id, p1.product_material_id, p1.product_qty
                    From pos_order_line_option a
                    inner join product_material_line p1 on a.option_id = p1.product_material_id
                    inner join product_material p2 on p1.product_material_id = p2.id
                    where a.line_id = ANY(@line_id); ";
        }
     
    }
}

