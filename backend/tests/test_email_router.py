"""Tests for the email router service (ported from eMailRouter.vbs)."""

from app.services.email_router_service import classify_email


class TestClassifyEmail:
    def test_rppr_match(self):
        rule = classify_email("eRA Commons: RPPR for Grant 5R01CA123456-05")
        assert rule is not None
        assert rule["category"] == "eRANotification"
        assert rule["sub_category"] == "RPPR Non-Compliance"

    def test_esnap_match(self):
        rule = classify_email("eSNAP Received at NIH for grant 1R01CA999999")
        assert rule is not None
        assert rule["category"] == "eRANotification"

    def test_jit_request(self):
        rule = classify_email("JIT Request for Grant 1R01CA111111-01")
        assert rule is not None
        assert rule["category"] == "JIT Info"
        assert rule["sub_category"] == "Reminder"

    def test_jit_submitted(self):
        rule = classify_email("JIT Documents Have Been Submitted for 1R01CA222222")
        assert rule is not None
        assert rule["category"] == "eRA Notification"
        assert rule["sub_category"] == "JIT Submitted"

    def test_closeout(self):
        rule = classify_email("Expiring Funds for grant R01CA333333")
        assert rule is not None
        assert rule["category"] == "Closeout"

    def test_no_cost_extension(self):
        rule = classify_email("No Cost Extension Submitted for 5R01CA444444-03")
        assert rule is not None
        assert rule["category"] == "eRANotification"
        assert rule["sub_category"] == "No Cost Extension"

    def test_ffr_rejection(self):
        rule = classify_email("FFR NOTIFICATION : REJECTED for grant R01CA555555")
        assert rule is not None
        assert rule["category"] == "Notification"
        assert rule["sub_category"] == "FFR Rejection"

    def test_ffr_rejection_skip_reply(self):
        """RE: prefixed emails should be skipped for FFR rejection."""
        rule = classify_email("RE: FFR NOTIFICATION : REJECTED for grant R01CA555555")
        assert rule is None

    def test_rppr_reminder(self):
        rule = classify_email("RPPR Reminder for grant 5R01CA666666-02")
        assert rule is not None
        assert rule["category"] == "RPPR"
        assert rule["sub_category"] == "Reminder"

    def test_supplement_requested(self):
        rule = classify_email("Supplement Requested through eRA for R01CA777777")
        assert rule is not None
        assert rule.get("forward_to_supplement") is True

    def test_undeliverable_skip(self):
        rule = classify_email("Undeliverable: Some email that bounced")
        assert rule is None

    def test_no_match(self):
        rule = classify_email("Random email about nothing relevant")
        assert rule is None

    def test_empty_subject(self):
        rule = classify_email("")
        assert rule is None

    def test_prior_approval(self):
        rule = classify_email("Prior Approval: Request for something")
        assert rule is not None
        assert rule.get("forward_to_post_award") is True

    def test_clinical_trials(self):
        rule = classify_email("ClinicalTrials.gov Results Reporting for NCT12345678")
        assert rule is not None
        assert rule["category"] == "CT.gov"

    def test_sbir_sttr(self):
        rule = classify_email("SBIR/STTR Foreign Risk Management for R43CA888888")
        assert rule is not None
        assert rule["category"] == "Funding"

    def test_fram_requested(self):
        rule = classify_email("FRAM Requested for grant 5R01CA999999-04")
        assert rule is not None
        assert rule["category"] == "FRAM"
        assert rule["sub_category"] == "Request"

    def test_application_withdrawn(self):
        rule = classify_email("CHANGE_NOTICE_FOR R01CA111111 - Application is withdrawn")
        assert rule is not None
        assert rule["category"] == "eRA Notification"
        assert rule["sub_category"] == "Application Withdrawn"
